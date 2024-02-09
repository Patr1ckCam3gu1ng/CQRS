namespace Broker.Cache;

public interface ICacheService
{
    Task Set(string key, string value, CancellationToken cancellationToken);

    Task<bool> TryGet(string key, out string value, CancellationToken cancellationToken);

    void ClearCache();
}