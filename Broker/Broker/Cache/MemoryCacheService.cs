using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Broker.Cache;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private CancellationTokenSource _cacheToken = new CancellationTokenSource();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task Set(string key, string value, CancellationToken cancellationToken)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetPriority(CacheItemPriority.Normal)
            .AddExpirationToken(new CancellationChangeToken(_cacheToken.Token));

        var item = _cache.Set(key, value, cacheEntryOptions);

        return Task.FromResult(item);
    }

    public Task<bool> TryGet(string key, out string value, CancellationToken cancellationToken)
    {
        var item = _cache.TryGetValue(key, out value);

        return Task.FromResult(item);
    }

    public void ClearCache()
    {
        _cacheToken.Cancel();
        _cacheToken.Dispose();
        _cacheToken = new CancellationTokenSource();
    }
}