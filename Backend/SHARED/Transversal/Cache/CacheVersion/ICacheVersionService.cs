namespace Transversal.Cache.CacheVersion
{
    public interface ICacheVersionService
    {
        Task<string> GetVersionAsync(string group);
        Task IncrementVersionAsync(string group);
    }
}
