using Microsoft.AspNetCore.Mvc.Filters;
using Transversal.Cache.CacheVersion;

namespace Transversal.Cache.Builder
{
    internal class CacheKeyBuilder
    {
        public static async Task<string> BuildAsync(
        string template,
        ActionExecutingContext context,
        bool perUser,
        string? versionGroup,
        ICacheVersionService versionService)
        {
            var key = template;

            foreach (var arg in context.ActionArguments)
            {
                key = key.Replace($"{{{arg.Key}}}", arg.Value?.ToString());
            }

            var query = context.HttpContext.Request.QueryString.ToString();
            if (!string.IsNullOrEmpty(query))
                key += $"|q:{query}";

            if (perUser && context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                key += $"|u:{context.HttpContext.User.Identity.Name}";
            }

            if (!string.IsNullOrEmpty(versionGroup))
            {
                var version = await versionService.GetVersionAsync(versionGroup);
                key = $"v{version}:{key}";
            }

            return key;
        }
    }
}
