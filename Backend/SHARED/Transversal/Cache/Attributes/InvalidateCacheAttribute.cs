using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transversal.Cache.CacheVersion;
using ZiggyCreatures.Caching.Fusion;

namespace Transversal.Cache.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class InvalidateCacheAttribute : Attribute, IAsyncActionFilter
    {
        public string[] Keys { get; }
        public string[] Tags { get; set; } = Array.Empty<string>();

        public InvalidateCacheAttribute(params string[] keys)
        {
            Keys = keys;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            if (executed.Exception == null)
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IFusionCache>();
                var versionService = context.HttpContext.RequestServices.GetRequiredService<ICacheVersionService>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidateCacheAttribute>>();

                if (Keys.Length > 0)
                {
                    foreach (var key in Keys)
                    {
                        await cache.RemoveAsync(key);
                        logger.LogInformation("Cache invalidated: {Key}", key);
                    }
                }

                if (Tags.Length > 0)
                {
                    foreach (var tag in Tags)
                    {
                        await versionService.IncrementVersionAsync(tag);
                        logger.LogInformation("Cache tag invalidated: {Tag}", tag);
                    }
                }
            }
        }
    }
}