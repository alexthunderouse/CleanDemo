Act as a Senior .NET Developer. Create a .NET 10 Worker Service project designed for background processing and scheduled tasks.

## Project Structure

Place the Worker in the Presentation layer:
```
src/
├── Core/
│   └── Application/               # Shared business logic, Commands/Handlers
├── Infrastructure/
│   └── Persistence/               # Data access
└── Presentation/
    └── Workers/
        └── ReportWorker/          # Worker service project
            ├── Configuration/     # Service configurations
            ├── Jobs/              # Coravel job implementations
            └── Options/           # Options classes
```

## Core Requirements

### Job Scheduling
- Use Coravel for task scheduling
- Define jobs in the Jobs/ folder
- Sample: DataSyncJob that runs every 10 minutes

### Architecture
- Jobs should delegate to Application layer handlers via MediatR
- Use `ISender` to send commands from jobs
- Maintain clean separation: Job → Command → Handler → Repository

### Configuration (Options Pattern)
Options classes must include `const SectionName`:
```csharp
public class DataSyncJobOptions
{
    public const string SectionName = "DataSyncJob";
    public string CronSchedule { get; set; } = "*/10 * * * *";
    public int BatchSize { get; set; } = 100;
}
```

Register with: `services.Configure<DataSyncJobOptions>(config.GetSection(DataSyncJobOptions.SectionName))`

### Observability
- Serilog with Graylog sink (GELF)
- Use structured logging with placeholders (never string interpolation):
```csharp
_logger.LogInformation("Processing batch {BatchId} with {ItemCount} items", batchId, count);
```
- OpenTelemetry for worker instrumentation
- Log job start/completion with execution duration

### Health Checks
Implement health check endpoint for orchestrator monitoring.

### Resilience
- Wrap job logic in Polly retry policy
- Handle transient database/network failures
- Log retry attempts with structured logging

### Error Handling
- Use domain exceptions from Core/Domain/Exceptions
- Log errors with context: `_logger.LogError(ex, "Job {JobName} failed for batch {BatchId}", jobName, batchId)`

### Testing
Provide unit tests using xUnit, NSubstitute, and FluentAssertions for:
- Job execution logic
- Command handlers

Show the Program.cs registration for Coravel and the DataSyncJob implementation.