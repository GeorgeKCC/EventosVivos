using Microsoft.Extensions.Caching.Distributed;

namespace Transversal.Cache.CacheVersion
{
    internal class CacheVersionService(IDistributedCache cache) : ICacheVersionService
    {
        private readonly IDistributedCache _cache = cache;

        public async Task<string> GetVersionAsync(string group)
        {

            var key = $"v:{group}";
            var version = await _cache.GetStringAsync(key);

            if (version == null)
            {
                version = "1";
                await _cache.SetStringAsync(key, version);
            }

            return version;
        }

        public async Task IncrementVersionAsync(string group)
        {
            var key = $"v:{group}";
            var version = await GetVersionAsync(group);

            var next = (int.Parse(version) + 1).ToString();

            await _cache.SetStringAsync(key, next);
        }
    }
}
