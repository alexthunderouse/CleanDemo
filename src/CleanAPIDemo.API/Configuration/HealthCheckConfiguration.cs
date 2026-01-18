namespace CleanAPIDemo.API.Configuration;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static WebApplication UseHealthCheckConfiguration(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}
