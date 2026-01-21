# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY CleanAPIDemo.slnx .
COPY src/CleanAPIDemo.Domain/CleanAPIDemo.Domain.csproj src/CleanAPIDemo.Domain/
COPY src/CleanAPIDemo.Application/CleanAPIDemo.Application.csproj src/CleanAPIDemo.Application/
COPY src/CleanAPIDemo.Infrastructure/CleanAPIDemo.Infrastructure.csproj src/CleanAPIDemo.Infrastructure/
COPY src/CleanAPIDemo.API/CleanAPIDemo.API.csproj src/CleanAPIDemo.API/

# Restore dependencies
RUN dotnet restore src/CleanAPIDemo.API/CleanAPIDemo.API.csproj

# Copy source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/CleanAPIDemo.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://cleanapi:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "CleanAPIDemo.API.dll"]
