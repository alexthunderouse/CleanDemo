using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace CleanAPIDemo.Worker.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this HostApplicationBuilder builder)
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
}
