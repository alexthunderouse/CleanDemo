using System.Net;
using System.Text;
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

    public static IServiceCollection AddHealthCheckEndpoint(this IServiceCollection services, IConfiguration configuration)
    {
        var port = configuration.GetValue("HealthCheck:Port", 5050);
        services.AddHostedService(sp => new HealthCheckHttpListener(
            sp.GetRequiredService<HealthCheckService>(),
            port,
            sp.GetRequiredService<ILogger<HealthCheckHttpListener>>()));

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

public class HealthCheckHttpListener : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly int _port;
    private readonly ILogger<HealthCheckHttpListener> _logger;

    public HealthCheckHttpListener(
        HealthCheckService healthCheckService,
        int port,
        ILogger<HealthCheckHttpListener> logger)
    {
        _healthCheckService = healthCheckService;
        _port = port;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://+:{_port}/health/");

        try
        {
            listener.Start();
            _logger.LogInformation("Health check endpoint listening on port {Port}", _port);

            while (!stoppingToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync().WaitAsync(stoppingToken);
                _ = HandleRequestAsync(context, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Health check endpoint shutting down");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check endpoint failed");
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
            var statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
            var response = $"{{\"status\":\"{report.Status}\"}}";

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var buffer = Encoding.UTF8.GetBytes(response);
            await context.Response.OutputStream.WriteAsync(buffer, cancellationToken);
        }
        finally
        {
            context.Response.Close();
        }
    }
}
