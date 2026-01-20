# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files
COPY src/CleanAPIDemo.Domain/CleanAPIDemo.Domain.csproj CleanAPIDemo.Domain/
COPY src/CleanAPIDemo.Application/CleanAPIDemo.Application.csproj CleanAPIDemo.Application/
COPY src/CleanAPIDemo.Infrastructure/CleanAPIDemo.Infrastructure.csproj CleanAPIDemo.Infrastructure/
COPY src/CleanAPIDemo.Worker/CleanAPIDemo.Worker.csproj CleanAPIDemo.Worker/

# Restore dependencies
RUN dotnet restore CleanAPIDemo.Worker/CleanAPIDemo.Worker.csproj

# Copy source code
COPY src/CleanAPIDemo.Domain/ CleanAPIDemo.Domain/
COPY src/CleanAPIDemo.Application/ CleanAPIDemo.Application/
COPY src/CleanAPIDemo.Infrastructure/ CleanAPIDemo.Infrastructure/
COPY src/CleanAPIDemo.Worker/ CleanAPIDemo.Worker/

# Build and publish
WORKDIR /src/CleanAPIDemo.Worker
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r workergroup && useradd -r -g workergroup workeruser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R workeruser:workergroup /app

# Switch to non-root user
USER workeruser

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5050/health/ || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "CleanAPIDemo.Worker.dll"]
