using Broker.Cache;
using Contracts.Repositories.Client;
using Contracts.Services.Interfaces;
using Contracts.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;

namespace Contracts.Services;

public class RetryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RetryQueue _retryQueue;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _waitInterval;

    public RetryBackgroundService(IServiceProvider serviceProvider, RetryQueue retryQueue, IConfiguration config)
    {
        _waitInterval = config["BackgroundService:WaitIntervalInMilliSeconds"] ?? throw new NullReferenceException();
        var maxRetry = config["BackgroundService:MaximumRetryCount"] ?? throw new NullReferenceException();

        _serviceProvider = serviceProvider;
        _retryQueue = retryQueue;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                Convert.ToInt32(maxRetry),
                _ => TimeSpan.FromMilliseconds(Convert.ToInt32(_waitInterval)));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_retryQueue.TryDequeue(out Func<IClientProcessorService, IClientRepository, ICacheService, CancellationToken, Task> operation))
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var clientProcessor = scope.ServiceProvider.GetRequiredService<IClientProcessorService>();
                    var clientRepo = scope.ServiceProvider.GetRequiredService<IClientRepository>();
                    var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                    await operation(clientProcessor, clientRepo, cache, cancellationToken);
                });
            }

            await Task.Delay(Convert.ToInt32(_waitInterval), cancellationToken);
        }
    }
}