using System.Collections.Concurrent;
using Broker.Cache;
using Contracts.Repositories.Client;
using Contracts.Services.Interfaces;

namespace Contracts.Utilities;

public class RetryQueue
{
    private ConcurrentQueue<Func<IClientProcessorService, IClientRepository, ICacheService, CancellationToken, Task>> _queue
        = new ConcurrentQueue<Func<IClientProcessorService, IClientRepository, ICacheService, CancellationToken, Task>>();

    public void Enqueue(Func<IClientProcessorService, IClientRepository, ICacheService, CancellationToken, Task> operation)
    {
        _queue.Enqueue(operation);
    }

    public bool TryDequeue(out Func<IClientProcessorService, IClientRepository, ICacheService, CancellationToken, Task> operation)
    {
        return _queue.TryDequeue(out operation);
    }
}