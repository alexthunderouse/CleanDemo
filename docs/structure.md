src/
├── Common/                        # Shared by all layers
│   ├── Extensions/                  # StringExtensions, ServiceCollectionExtensions
│   └── Constants/
│
├── Core/
│   ├── Domain/                      # Enterprise Rules
│   │   ├── Entities/ | ValueObjects/ | Enums/ | Events/ | Exceptions/ | Interfaces/
│   │
│   └── Application/                 # Business Rules
│       ├── Common/                  # MediatR Behaviors, Shared Interfaces
│       ├── Mappings/                # Mapperly Mappers
│       ├── Features/                # Vertical Slices
│       │   └── Users/
│       │       ├── v1/              # Namespace: Application.Features.Users.v1
│       │       │   ├── Commands/    # CreateUserCommand, CreateUserHandler
│       │       │   ├── Queries/     # GetUserQuery
│       │       │   └── UserDto.cs   <-- Plain name, distinct namespace
│       │       └── v2/              # Namespace: Application.Features.Users.v2
│       │           ├── Commands/
│       │           └── UserDto.cs   <-- Updated schema
│       └── DependencyInjection.cs
│
├── Infrastructure/                # External Concerns
│   ├── Persistence/                 # EF Core, Migrations, Interceptors
│   ├── Messaging/                   # Message Broker (RabbitMQ/MassTransit)
│   ├── ExternalClients/             # 3rd Party API Clients
│   └── DependencyInjection.cs
│
├── Presentation/                  # Entry Points (Multiple .csproj)
│   ├── Project.Public.Api/          # External API
│   │   ├── Controllers/
│   │   │   ├── v1/                  # Namespace: Public.Api.Controllers.v1
│   │   │   └── v2/                  # Namespace: Public.Api.Controllers.v2
│   │   ├── Middleware/ | Filters/
│   │   └── Program.cs
│   ├── Project.Admin.Api/           # Admin/Internal API
│   └── Project.Workers/             # Background Services
│       ├── IdentityWorker/
│       └── ReportWorker/
│
deploy/                              # Deployment Configuration
├── nginx/
│   └── nginx.conf                   # Reverse proxy routing rules
└── docker/
    └── docker-compose.yml