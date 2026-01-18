using CleanAPIDemo.Worker.Options;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace CleanAPIDemo.Worker.Configuration;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this HostApplicationBuilder builder)
    {
        var graylogOptions = builder.Configuration
            .GetSection(GraylogOptions.SectionName)
            .Get<GraylogOptions>() ?? new GraylogOptions();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "CleanAPIDemo.Worker")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = graylogOptions.Host,
                Port = graylogOptions.Port,
                TransportType = TransportType.Udp,
                Facility = graylogOptions.Facility
            })
            .CreateLogger();

        builder.Services.AddSerilog();
    }
}
