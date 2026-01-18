Act as a Senior .NET Developer. Create a .NET 10 Worker Service project designed for background processing and scheduled tasks.

Core Requirements:

Job Scheduling: Use Coravel to handle task scheduling. Define a sample job DataSyncJob that runs every 10 minutes.

Architecture: Maintain a clean separation of concerns. The Job should call a Service layer for business logic.

Observability: Setup Serilog with a Graylog sink (GELF).

Integrate OpenTelemetry for background worker instrumentation.

Health Checks: Implement a Health Check endpoint (using a small background HTTP listener or a file-based check) so the orchestrator knows the service is alive.

Configuration: Use the Options Pattern for job settings (e.g., cron schedules or API keys) in appsettings.json.

Resilience: Wrap the Job's internal logic in a Polly retry policy to handle transient database or network issues during execution.

Show the Program.cs registration for Coravel and the DataSyncJob implementation.