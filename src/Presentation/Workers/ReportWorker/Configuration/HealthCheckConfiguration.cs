using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CleanAPIDemo.Worker.Configuration;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("worker_health");

        return services;
    }
}

public class WorkerHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Worker is running"));
    }
}
