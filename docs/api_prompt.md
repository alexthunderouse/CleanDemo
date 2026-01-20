Act as a Senior .NET Architect. Generate a .NET 10 Web API solution following Clean Architecture principles.

## Project Structure

Follow this folder organization:
```
src/
├── Common/                        # Shared utilities (Extensions, Constants)
├── Core/
│   ├── Domain/                    # Entities, Exceptions, Interfaces
│   └── Application/               # Features, Mappings, Common behaviors
│       ├── Common/Behaviors/      # MediatR Pipeline Behaviors
│       ├── Mappings/              # Mapperly Mappers
│       └── Features/{Feature}/v1/ # Versioned Commands, Queries, DTOs
├── Infrastructure/
│   ├── Persistence/               # EF Core, Repositories
│   └── Messaging/                 # Message broker implementations
└── Presentation/
    └── Public.Api/                # Controllers, Middleware
        └── Controllers/v1/        # Versioned controllers
```

## Core Requirements

### Data & Mapping
- Use EF Core with Repository and Unit of Work patterns
- Use Mapperly for high-performance mapping (register as Singleton)
- DTOs must use C# records for immutability

### MediatR & Validation
- Commands/Queries must use C# records: `public record CreateProductCommand(...) : IRequest<ProductDto>`
- Controllers must inject `ISender` (not `IMediator`)
- FluentValidation error messages must use snake_case codes: `.WithMessage("name_required")`
- Implement ValidationBehavior as MediatR Pipeline Behavior

### Exception Handling
Create custom exception hierarchy in Domain/Exceptions:
- `DomainException` (base) with Type, Code, StatusCode properties
- `NotFoundException` (404)
- `DuplicateEntryException` (409)
- `BusinessRuleException` (422)

Global Exception Middleware must return RFC 7807 responses with:
- `type`, `title`, `status`, `detail`, `instance`
- `code` - snake_case translation key
- `traceId` - for log correlation
- `timestamp` - UTC time
- `errors` - validation field errors (for 400)

### API Versioning
- URL-based versioning: `/api/v{version:apiVersion}/[controller]`
- Use `[ApiVersion("1.0")]` attribute on controllers
- Version-specific DTOs in separate namespaces

### Resilience
Configure Polly v8 Resilience Pipeline with:
- 3-step Exponential Retry
- Circuit Breaker for outbound HTTP calls

### Observability
- Serilog with Graylog sink (GELF)
- Use structured logging with placeholders: `_logger.LogInformation("User {UserId} created", userId)`
- Never use string interpolation in log messages

### Documentation
Use Scalar for API documentation instead of Swagger.

### Testing
Provide sample Unit Tests using xUnit, NSubstitute, and FluentAssertions.

Create a 'Products' feature (GetById and Create) with v1 implementation as an example.