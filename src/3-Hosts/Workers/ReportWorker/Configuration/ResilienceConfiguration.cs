using CleanAPIDemo.Worker.Options;
using Polly;
using Polly.Retry;
using Serilog;

namespace CleanAPIDemo.Worker.Configuration;

public static class ResilienceConfiguration
{
    public static IServiceCollection AddResilienceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(DataSyncJobOptions.SectionName)
            .Get<DataSyncJobOptions>() ?? new DataSyncJobOptions();

        services.AddResiliencePipeline("job-retry", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>()
                    .Handle<InvalidOperationException>(),
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = TimeSpan.FromSeconds(options.RetryDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Log.Warning("Retry {AttemptNumber} after {RetryDelay}ms due to {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            });
        });

        return services;
    }
}
