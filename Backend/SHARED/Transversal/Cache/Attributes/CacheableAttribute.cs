using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transversal.Cache.Builder;
using Transversal.Cache.CacheVersion;
using ZiggyCreatures.Caching.Fusion;

namespace Transversal.Cache.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class CacheableAttribute(string keyTemplate) : Attribute, IAsyncActionFilter
    {
        public string KeyTemplate { get; } = keyTemplate;
        public int DurationSeconds { get; set; } = 300;
        public string[] Tags { get; set; } = [];
        public bool PerUser { get; set; } = false;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IFusionCache>();
            var versionService = context.HttpContext.RequestServices.GetRequiredService<ICacheVersionService>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CacheableAttribute>>();

            var key = await CacheKeyBuilder.BuildAsync(
                KeyTemplate,
                context,
                PerUser,
                Tags.FirstOrDefault(),
                versionService);

            var cachedEntry = await cache.TryGetAsync<object>(key);

            if (cachedEntry.HasValue)
            {
                logger.LogInformation("CACHE HIT: {Key}", key);
                context.Result = new OkObjectResult(cachedEntry.Value);
                return;
            }

            logger.LogInformation("CACHE MISS: {Key}", key);
            var executed = await next();

            if (executed.Exception != null && !executed.ExceptionHandled)
            {
                logger.LogWarning("Exception occurred, not caching: {Key}", key);
                return;
            }

            if (executed.Result is ObjectResult objResult)
            {
                var statusCode = objResult.StatusCode ?? context.HttpContext.Response.StatusCode;

                if (statusCode < 400 && objResult.Value != null)
                {
                    try
                    {
                        await cache.SetAsync(
                            key,
                            objResult.Value,
                            new FusionCacheEntryOptions
                            {
                                Duration = TimeSpan.FromSeconds(DurationSeconds)
                            });

                        logger.LogInformation("Cached successful response: {Key} (Status: {StatusCode})", key, statusCode);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to cache response for {Key}: {Error}", key, ex.Message);
                    }
                }
                else
                {
                    logger.LogWarning("Not caching error response: {Key} (Status: {StatusCode})", key, statusCode);
                }
            }
        }
    }
}