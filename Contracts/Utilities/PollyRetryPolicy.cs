using Polly;
using Polly.Retry;

namespace Contracts.Utilities;

public static class PollyRetryPolicy
{
    public static AsyncRetryPolicy CreatePolicy(int maxRetryAttempts, TimeSpan pauseBetweenFailures)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(maxRetryAttempts, _ => pauseBetweenFailures);
    }
}