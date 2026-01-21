using CleanAPIDemo.API.Configuration;
using CleanAPIDemo.API.Middleware;
using CleanAPIDemo.Application;
using CleanAPIDemo.Infrastructure;
using CleanAPIDemo.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.ConfigureSerilog();

builder.Services
    .AddApplication()
    .AddInfrastructureInMemory()
    .AddApiVersioningConfiguration()
    .AddResilienceConfiguration()
    .AddOpenTelemetryConfiguration()
    .AddHealthCheckConfiguration()
    .AddOpenApi()
    .AddControllers();

var app = builder.Build();

// Seed the database
DatabaseSeeder.SeedDatabase(app.Services);

// Configure middleware pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHealthCheckConfiguration();
app.UseSerilogRequestLoggingConfiguration();
app.UseHttpsRedirection();

// Map OpenAPI and Scalar documentation
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("CleanAPIDemo API")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.WithTheme(ScalarTheme.Moon);
});

app.UseAuthorization();
app.MapControllers();

Log.Information("Starting Clean API Demo application");
app.Run();
