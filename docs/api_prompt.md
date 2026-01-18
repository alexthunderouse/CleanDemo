Act as a Senior .NET Architect. Generate a .NET 10 Web API solution following Clean Architecture (Domain, Application, Infrastructure, and API projects).

Core Requirements:

Data & Mapping: Use EF Core with the Repository pattern. Use Mapperly for high-performance mapping between Entities and DTOs.

Logic & Validation: Implement the Mediator pattern using MediatR. All requests must be validated using FluentValidation via a MediatR Pipeline Behavior.

Resilience: Configure a Polly v8 Resilience Pipeline in Program.cs that includes a 3-step Exponential Retry and a Circuit Breaker for outbound HTTP calls.

Observability: Setup Serilog with a Graylog sink (GELF).

API Standards: Use Scalar for API documentation instead of default Swagger.

Implement a Global Exception Middleware that returns errors using the Problem Details (RFC 7807) standard.

Enable API Versioning (URL-based).

Testing: Provide one sample Unit Test using xUnit and NSubstitute for an Application Layer command.

Create a 'Products' feature (GetById and Create) as an example.