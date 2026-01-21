# Tools and Frameworks Reference

This document provides an overview of all tools, libraries, and frameworks used in the CleanAPIDemo project with explanations and links to official documentation.

---

## Table of Contents

- [Framework](#framework)
- [Architecture Patterns](#architecture-patterns)
- [Application Layer](#application-layer)
- [Data Access](#data-access)
- [API Development](#api-development)
- [Resilience & Fault Handling](#resilience--fault-handling)
- [Observability](#observability)
- [Background Processing](#background-processing)
- [Testing](#testing)
- [DevOps & Containerization](#devops--containerization)

---

## Framework

### .NET 10.0

The latest version of Microsoft's cross-platform framework for building modern applications. Provides the runtime, libraries, and tools for building web APIs, services, and more.

- **Documentation:** https://learn.microsoft.com/en-us/dotnet/
- **What's New:** https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview

### ASP.NET Core

A cross-platform, high-performance framework for building modern, cloud-enabled, Internet-connected applications including Web APIs and background services.

- **Documentation:** https://learn.microsoft.com/en-us/aspnet/core/
- **Web API Tutorial:** https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api

---

## Architecture Patterns

### Clean Architecture

An architectural pattern that organizes code into concentric layers with dependencies pointing inward. The inner layers contain business logic and are independent of external concerns like databases or UI.

**Project Structure:**
- `Domain` - Entities, interfaces (innermost layer)
- `Application` - Use cases, business logic
- `Infrastructure` - Data access, external services
- `Presentation` - API controllers, workers (outermost layer)

- **Overview:** https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **Microsoft Guide:** https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture

### CQRS (Command Query Responsibility Segregation)

A pattern that separates read operations (queries) from write operations (commands). Each operation has its own model optimized for its specific purpose.

- **Overview:** https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs
- **Martin Fowler:** https://martinfowler.com/bliki/CQRS.html

---

## Application Layer

### MediatR

A simple mediator implementation in .NET that enables in-process messaging with no dependencies. Used to implement CQRS pattern by dispatching commands and queries to their respective handlers.

```csharp
// Command example
public record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;

// Handler
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct) { ... }
}
```

- **GitHub:** https://github.com/jbogard/MediatR
- **Documentation:** https://github.com/jbogard/MediatR/wiki

### FluentValidation

A .NET library for building strongly-typed validation rules. Provides a fluent interface for defining validation logic separate from your models.

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

- **Documentation:** https://docs.fluentvalidation.net/
- **GitHub:** https://github.com/FluentValidation/FluentValidation

### Riok.Mapperly

A source generator for object-to-object mapping. Unlike runtime mappers (AutoMapper), Mapperly generates mapping code at compile time, resulting in zero runtime overhead.

```csharp
[Mapper]
public partial class ProductMapper
{
    public ProductDto ToProductDto(Product product) => new(...);
}
```

- **Documentation:** https://mapperly.riok.app/
- **GitHub:** https://github.com/riok/mapperly

---

## Data Access

### Entity Framework Core

Microsoft's modern object-relational mapper (ORM) for .NET. Enables developers to work with databases using .NET objects, eliminating most data-access code.

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
}
```

- **Documentation:** https://learn.microsoft.com/en-us/ef/core/
- **Getting Started:** https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app

### EF Core SQL Server Provider

The SQL Server database provider for Entity Framework Core. Enables EF Core to work with Microsoft SQL Server databases.

- **Documentation:** https://learn.microsoft.com/en-us/ef/core/providers/sql-server/

### EF Core InMemory Provider

An in-memory database provider for Entity Framework Core. Useful for testing and development without requiring a real database.

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

- **Documentation:** https://learn.microsoft.com/en-us/ef/core/providers/in-memory/

---

## API Development

### Asp.Versioning

A library for API versioning in ASP.NET Core. Supports URL path, query string, header, and media type versioning strategies.

```csharp
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase { }
```

- **GitHub:** https://github.com/dotnet/aspnet-api-versioning
- **Documentation:** https://github.com/dotnet/aspnet-api-versioning/wiki

### Scalar

A modern, beautiful API documentation UI for OpenAPI specifications. An alternative to Swagger UI with a cleaner interface and better developer experience.

```csharp
app.MapScalarApiReference(options =>
{
    options.WithTitle("CleanAPIDemo API")
        .WithTheme(ScalarTheme.Moon);
});
```

- **Website:** https://scalar.com/
- **GitHub:** https://github.com/scalar/scalar
- **ASP.NET Core Integration:** https://github.com/scalar/scalar/tree/main/packages/scalar.aspnetcore

### Microsoft.AspNetCore.OpenApi

Built-in support for generating OpenAPI documentation in ASP.NET Core. Automatically generates OpenAPI specs from your controllers and endpoints.

- **Documentation:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview

---

## Resilience & Fault Handling

### Polly

A .NET resilience and transient-fault-handling library. Allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback.

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

- **Documentation:** https://www.thepollyproject.org/
- **GitHub:** https://github.com/App-vNext/Polly

### Microsoft.Extensions.Http.Resilience

Microsoft's official integration of Polly with HttpClientFactory. Provides pre-configured resilience pipelines for HTTP requests.

```csharp
services.AddHttpClient("MyClient")
    .AddStandardResilienceHandler();
```

- **Documentation:** https://learn.microsoft.com/en-us/dotnet/core/resilience/
- **HTTP Resilience:** https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience

---

## Observability

### Serilog

A diagnostic logging library for .NET with support for structured logging. Logs are recorded as structured data that can be queried and analyzed.

```csharp
Log.Information("Processing order {OrderId} for {Customer}", orderId, customer);
```

**Packages Used:**
- `Serilog.AspNetCore` - ASP.NET Core integration
- `Serilog.Sinks.Console` - Console output
- `Serilog.Sinks.Graylog` - Graylog integration
- `Serilog.Enrichers.Environment` - Environment enrichment
- `Serilog.Enrichers.Thread` - Thread information enrichment

- **Documentation:** https://serilog.net/
- **GitHub:** https://github.com/serilog/serilog
- **ASP.NET Core:** https://github.com/serilog/serilog-aspnetcore

### Graylog

A centralized log management platform. Used via Serilog sink to aggregate and analyze logs from multiple services.

- **Website:** https://graylog.org/
- **Documentation:** https://go2docs.graylog.org/

---

## Background Processing

### Coravel

A near-zero config library for .NET that makes task scheduling, queuing, caching, and more easy. Used for scheduling background jobs in worker services.

```csharp
services.AddScheduler();

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<ReportJob>()
        .EveryMinute();
});
```

- **Documentation:** https://docs.coravel.net/
- **GitHub:** https://github.com/jamesmh/coravel

---

## Testing

### xUnit

A free, open-source, community-focused unit testing framework for .NET. The most popular testing framework in the .NET ecosystem.

```csharp
public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsProduct() { ... }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPrice_ThrowsException(decimal price) { ... }
}
```

- **Documentation:** https://xunit.net/
- **GitHub:** https://github.com/xunit/xunit

### FluentAssertions

A library that provides a fluent API for asserting the results of unit tests. Makes tests more readable and provides better failure messages.

```csharp
result.Should().NotBeNull();
result.Name.Should().Be("Expected Name");
products.Should().HaveCount(5).And.OnlyContain(p => p.Price > 0);
```

- **Documentation:** https://fluentassertions.com/
- **GitHub:** https://github.com/fluentassertions/fluentassertions

### NSubstitute

A friendly substitute for .NET mocking libraries. Creates mock objects for interfaces and virtual methods with a simple, readable syntax.

```csharp
var repository = Substitute.For<IProductRepository>();
repository.GetByIdAsync(Arg.Any<Guid>()).Returns(expectedProduct);
```

- **Documentation:** https://nsubstitute.github.io/
- **GitHub:** https://github.com/nsubstitute/NSubstitute

### BenchmarkDotNet

A powerful .NET library for benchmarking. Transforms methods into benchmarks, tracks results, and provides detailed reports with statistical analysis.

```csharp
[MemoryDiagnoser]
public class MapperBenchmarks
{
    [Benchmark]
    public void ToProductDto() => _mapper.ToProductDto(_product);
}
```

- **Documentation:** https://benchmarkdotnet.org/
- **GitHub:** https://github.com/dotnet/BenchmarkDotNet

---

## DevOps & Containerization

### Docker

A platform for developing, shipping, and running applications in containers. Containers package an application with all its dependencies into a standardized unit.

- **Documentation:** https://docs.docker.com/
- **Get Started:** https://docs.docker.com/get-started/
- **.NET Docker Guide:** https://learn.microsoft.com/en-us/dotnet/core/docker/introduction

### Docker Compose

A tool for defining and running multi-container Docker applications. Uses YAML files to configure application services.

```yaml
services:
  api:
    build: .
    ports:
      - "5000:8080"
    depends_on:
      - sqlserver
```

- **Documentation:** https://docs.docker.com/compose/
- **Compose File Reference:** https://docs.docker.com/compose/compose-file/

### SQL Server (Container)

Microsoft SQL Server running in a Docker container. Used as the production database for the application.

- **Docker Hub:** https://hub.docker.com/_/microsoft-mssql-server
- **Documentation:** https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker

---

## Quick Reference Table

| Category | Tool | Purpose |
|----------|------|---------|
| Framework | .NET 10.0 | Runtime and SDK |
| Framework | ASP.NET Core | Web API framework |
| Pattern | MediatR | CQRS / Mediator |
| Validation | FluentValidation | Input validation |
| Mapping | Mapperly | Object mapping |
| ORM | EF Core | Database access |
| Database | SQL Server | Data storage |
| API Docs | Scalar | OpenAPI UI |
| Versioning | Asp.Versioning | API versioning |
| Resilience | Polly | Fault handling |
| Logging | Serilog | Structured logging |
| Scheduling | Coravel | Background jobs |
| Testing | xUnit | Unit tests |
| Mocking | NSubstitute | Test mocks |
| Assertions | FluentAssertions | Test assertions |
| Benchmarking | BenchmarkDotNet | Performance tests |
| Containers | Docker | Containerization |

---

## Learning Path Recommendations

### Beginner
1. .NET and C# fundamentals
2. ASP.NET Core Web API basics
3. Entity Framework Core
4. xUnit testing

### Intermediate
5. Clean Architecture principles
6. CQRS with MediatR
7. FluentValidation
8. Docker basics

### Advanced
9. Polly resilience patterns
10. BenchmarkDotNet performance optimization
11. Advanced EF Core (migrations, optimization)
