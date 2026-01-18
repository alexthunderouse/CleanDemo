using CleanAPIDemo.Application;
using CleanAPIDemo.Infrastructure;
using CleanAPIDemo.Worker.Configuration;
using CleanAPIDemo.Worker.Jobs;
using CleanAPIDemo.Worker.Options;
using Coravel;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog with Graylog sink
builder.ConfigureSerilog();

// Configure Options
builder.Services.Configure<DataSyncJobOptions>(
    builder.Configuration.GetSection(DataSyncJobOptions.SectionName));
builder.Services.Configure<GraylogOptions>(
    builder.Configuration.GetSection(GraylogOptions.SectionName));

// Add Application and Infrastructure layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));

// Add Coravel Scheduler and Jobs
builder.Services.AddScheduler();
builder.Services.AddTransient<DataSyncJob>();

// Add Configuration
builder.Services
    .AddOpenTelemetryConfiguration(builder.Configuration)
    .AddHealthCheckConfiguration()
    .AddHealthCheckEndpoint(builder.Configuration)
    .AddResilienceConfiguration(builder.Configuration);

var host = builder.Build();

// Configure Coravel Job Scheduling
host.Services.UseScheduler(scheduler =>
{
    var options = builder.Configuration
        .GetSection(DataSyncJobOptions.SectionName)
        .Get<DataSyncJobOptions>() ?? new DataSyncJobOptions();

    // Schedule job using cron expression for configurable interval
    // Default: every 10 minutes (*/10 * * * *)
    var cronExpression = $"*/{options.IntervalMinutes} * * * *";

    scheduler
        .Schedule<DataSyncJob>()
        .Cron(cronExpression)
        .PreventOverlapping(nameof(DataSyncJob));
});

Log.Information("Starting CleanAPIDemo Worker Service");
host.Run();
