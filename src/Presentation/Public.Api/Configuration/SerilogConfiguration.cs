using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace CleanAPIDemo.API.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    HostnameOrAddress = builder.Configuration["Graylog:Host"] ?? "localhost",
                    Port = builder.Configuration.GetValue("Graylog:Port", 12201),
                    TransportType = Enum.Parse<TransportType>(
                        builder.Configuration["Graylog:Transport"] ?? "Udp"),
                    Facility = builder.Configuration["Graylog:Facility"] ?? builder.Environment.ApplicationName
                });
        });
    }

    public static void UseSerilogRequestLoggingConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? "unknown");
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            };

            // Don't log health checks
            options.GetLevel = (httpContext, elapsed, ex) =>
                httpContext.Request.Path.StartsWithSegments("/health")
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information;
        });
    }
}
