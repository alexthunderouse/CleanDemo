# Coding Standards

**SQL Server & C# Coding Standards and Best Practices**

*Last Updated: 2025-01*

---

## Table of Contents

### SQL Server Standards
1. [Schema Usage](#1-schema-usage)
2. [Table Naming](#2-table-naming)
3. [Column & Data Types](#3-column--data-types)
4. [Enum Tables](#4-enum-tables)
5. [Views & Functions](#5-views--functions)
6. [Stored Procedures](#6-stored-procedures)
7. [T-SQL Formatting](#7-t-sql-formatting)
8. [Commenting](#8-commenting)
9. [Indexing Strategy](#9-indexing-strategy)
10. [Transactions](#10-transactions)
11. [Dynamic SQL](#11-dynamic-sql)
12. [WITH (NOLOCK)](#12-with-nolock)
13. [Cursors](#13-cursors)
14. [SARGable Queries](#14-sargable-queries)
15. [SELECT *](#15-select-)
16. [Temporary Objects](#16-temporary-objects)
17. [Schema Binding](#17-schema-binding)

### C# Standards
1. [Naming Conventions](#c-1-naming-conventions)
2. [Clean Architecture](#c-2-clean-architecture)
3. [Formatting](#c-3-formatting)
4. [DTOs](#c-4-dtos)
5. [Async/Await](#c-5-asyncawait)
6. [Error Handling](#c-6-error-handling)
7. [Entity Framework Core](#c-7-entity-framework-core)
8. [MediatR](#c-8-mediatr)
9. [API Versioning](#c-9-api-versioning)
10. [Mapperly](#c-10-mapperly)
11. [Configuration](#c-11-configuration)
12. [Best Practices](#c-12-best-practices)
13. [Documentation](#c-13-documentation)
14. [DI Scope Rules](#c-14-di-scope-rules)
15. [Serilog & Logging Standards](#c-15-serilog--logging-standards)
16. [Graylog Integration](#c-16-graylog-integration)
17. [Background Jobs](#c-17-background-jobs)
18. [EMQX/MQTT](#c-18-emqxmqtt)
19. [Authorization](#c-19-authorization)
20. [Request Validation](#c-20-request-validation)
21. [Error Responses (RFC 7807)](#c-21-error-responses-rfc-7807)
22. [Custom Exceptions](#c-22-custom-exceptions)
23. [Error Handling Best Practices](#c-23-error-handling-best-practices)
24. [Unit Testing](#c-24-unit-testing)
25. [Observability](#c-25-observability)

---

# SQL Server (T-SQL) Coding Standards

## 1. Schema Usage

### Schema Organization

Schemas are the primary tool for organizing database objects. They should be used to logically group related objects, which improves navigation and helps in managing security permissions.

**Strategy:** Group objects by application feature or logical domain. Avoid using the default `dbo` schema for application objects.

### Cross-Schema Relationships

For objects that relate two different schemas (like a junction table), the object should be placed in the schema of the primary business entity.

```sql
-- Example: TB_CardIssueCategories linking Cards and IssueCategories
-- Should reside in Cards schema (primary entity)
Cards.TB_CardIssueCategories

-- Always include schema name when querying
SELECT *
FROM Verifications.TB_Applicants
```

---

## 2. Table Naming

### Table Naming Conventions

Consistent table naming ensures database objects are easily identifiable and maintainable.

| Rule | Description |
|------|-------------|
| **Prefix** | All table names must begin with the prefix `TB_` |
| **Casing** | Use UpperCamelCase (e.g., `TB_ApplicantSessions`) |
| **Clarity** | Names should be descriptive, unambiguous, and in plural form where appropriate |
| **Junction Tables** | For many-to-many relationships, combine the two entities with the primary entity first (e.g., `TB_ApplicantInternalNotes`) |

---

## 3. Column & Data Types

### Column Naming

| Element | Convention | Example |
|---------|------------|---------|
| **Casing** | Use UpperCamelCase for all column names | `FirstName` |
| **Primary Keys** | All primary keys should be named simply `ID` | `ID` |
| **Foreign Keys** | Name should reference the source table | `ApplicantID`, `CreatedByUserID` |
| **Clarity** | Avoid abbreviations | Use `DateOfBirth` instead of `DOB` |

### Standard Data Types

| Type | Usage | Notes |
|------|-------|-------|
| **Text** | `NVARCHAR(n)` | For all text data to support Unicode. Avoid `VARCHAR` |
| **Date & Time** | `DATETIME2(7)` | For all date and time storage. Avoid `DATETIME` |
| **Whole Numbers** | `INT` or `BIGINT` | Based on expected range |
| **Decimal Numbers** | `DECIMAL(p, s)` | For financial calculations |

---

## 4. Enum Tables

### "Enum" Tables for Static Values

To manage static sets of values (like statuses or types), dedicated tables must be created.

**Key Principles:**
- **Immutability (Recommended):** The primary key (`ID`) values must be treated as permanent
- **Mutable ID Exception:** If an ID might change, include an additional, immutable text-based `Key` column

### Enum Functions (Avoiding Magic Numbers)

```sql
-- For Immutable Enums (hardcoded ID)
CREATE FUNCTION Enums_Status_Pending()
RETURNS INT
AS
BEGIN
    RETURN 1
END
GO

-- For Mutable Enums (queries using Key column)
CREATE FUNCTION Enums_Status_Pending()
RETURNS INT
AS
BEGIN
    RETURN (
        SELECT ID
        FROM TB_Statuses
        WHERE [Key] = 'PENDING'
    )
END
GO

-- Usage in queries
WHERE StatusID = dbo.Enums_Status_Pending()
```

**Key Benefit:** The calling code is identical whether using immutable or mutable pattern. Never use magic numbers like `WHERE StatusID = 1`.

---

## 5. Views & Functions

### Views, Functions, and Reusable Logic

**Core Benefit:** Views and TVFs are composable - you can JOIN them to other tables and views.

| Type | Best For | Performance |
|------|----------|-------------|
| **View** | Non-parameterized, reusable query logic | Excellent |
| **Inline TVF (iTVF)** | Parameterized, reusable query logic | Just as performant as a view |
| **Multi-statement TVF (mTVF)** | AVOID using in joins | Known performance bottleneck |

---

## 6. Stored Procedures

### Stored Procedure & Object Naming

All stored procedures and functions must follow the `Entity_Action` pattern.

```sql
-- Single entity operations
Applicants_GetByID
Applicants_GetByGuid
Applicants_Insert
Applicants_Update
Applicants_Delete
Applicants_GetActive

-- Multiple entity operations (use primary entity)
ApplicantSessions_GetWithApplicantDetails
Orders_GetWithCustomerInfo

-- Batch operations
Applicants_ProcessPending
Orders_CalculateTotals
```

### Properly Formatted Stored Procedure

```sql
ALTER PROCEDURE dbo.SocketConnections_GetDisconnected
AS
BEGIN

    SET NOCOUNT ON;

    SELECT
        USC.ID,
        USC.UserID,
        U.[Guid] AS UserGuid,
        USC.FilterID,
        USC.[Page],
        USC.ConnectionGuid,
        USC.CreateTime,
        USC.SessionGuid
    FROM dbo.TB_UserSocketConnections USC WITH(NOLOCK)
    INNER JOIN dbo.TB_Users U WITH(NOLOCK) ON U.ID = USC.UserID
    WHERE
        USC.CreateTime <= DATEADD(SECOND, -60, SYSUTCDATETIME())

END
GO
```

### Views & Functions Naming

```sql
-- Views use VW_ prefix
VW_ActiveApplicants
VW_PendingOrders
VW_ApplicantSessionDetails

-- Scalar functions - Entity_Calculate/Get/Check pattern
Applicants_GetAge(@DateOfBirth DATE)
Orders_CalculateTotal(@OrderID INT)
Users_CheckPermission(@UserID INT, @Permission NVARCHAR(50))

-- Enum functions - special pattern
Enums_Status_Pending()
Enums_DocumentType_Passport()
```

> **Important:** Never use prefixes like `sp_` (reserved for system procedures), `proc_`, or `usp_`.

---

## 7. T-SQL Formatting

### Formatting Rules

| Rule | Description |
|------|-------------|
| **Keywords** | All T-SQL keywords must be in UPPERCASE (`SELECT`, `FROM`, `WHERE`) |
| **Aliases** | Table aliases should be derived from the first letters of the table's CamelCase name and must be in UPPERCASE |
| **Indentation** | All subqueries, joins, and logical blocks must be clearly indented |
| **Procedure Performance** | Every stored procedure must begin with `SET NOCOUNT ON;` |
| **NULL Handling** | Always use `IS NULL` or `IS NOT NULL` operators |
| **CTEs for Readability** | Use Common Table Expressions for complex queries |

### CTE Example

```sql
;WITH ActiveApplicants AS (
    SELECT
        ApplicantID,
        FirstName
    FROM Verifications.TB_Applicants
    WHERE IsActive = 1
)
SELECT
    AA.FirstName,
    APS.SessionDate
FROM ActiveApplicants AS AA
JOIN Verifications.TB_ApplicantSessions AS APS ON AA.ApplicantID = APS.ApplicantID;
```

---

## 8. Commenting

### Header Block (Required for all procedures, functions, views)

```sql
-- =============================================
-- Author: <Author Name>
-- Create date: <Create Date>
-- Description: Retrieves the active sessions for a given applicant.
-- =============================================
CREATE PROCEDURE dbo.Applicants_GetActiveSessions
AS
BEGIN

    -- Your procedure logic here

END
GO
```

### What to Comment

- Business rules and logic
- Non-obvious calculations or date manipulations
- Workarounds or temporary fixes (with ticket numbers)
- Complex JOIN conditions
- WHERE clause conditions that implement business rules
- Any hardcoded values and why they exist

---

## 9. Indexing Strategy

### Clustered Index (One per table)

Defines the physical storage order of data. This is almost always the Primary Key.

```sql
-- Created automatically with primary key
CREATE TABLE TB_Applicants (
    ID INT IDENTITY(1,1) PRIMARY KEY, -- This creates clustered index
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100)
)
```

### Mandatory Non-Clustered Indexes

```sql
-- 1. ALWAYS index foreign keys
CREATE NONCLUSTERED INDEX IX_ApplicantSessions_ApplicantID
ON TB_ApplicantSessions(ApplicantID);

-- 2. Index columns used in WHERE clauses
CREATE NONCLUSTERED INDEX IX_Applicants_Email
ON TB_Applicants(Email);

-- 3. Index columns used in JOINs (if not already FK)
CREATE NONCLUSTERED INDEX IX_Orders_CustomerCode
ON TB_Orders(CustomerCode);
```

> **Remember:** Every index speeds up reads but slows down writes (INSERT, UPDATE, DELETE). Find the right balance for your workload.

---

## 10. Transactions

### Transaction Pattern

Transactions are NOT always needed. Use them only when multiple operations must succeed or fail as a single unit.

```sql
BEGIN TRY
    BEGIN TRANSACTION;

    -- Operation 1: Deduct from account
    UPDATE Accounts.TB_Balances
    SET Balance = Balance - @Amount
    WHERE AccountID = @SourceAccountID;

    -- Operation 2: Add to account
    UPDATE Accounts.TB_Balances
    SET Balance = Balance + @Amount
    WHERE AccountID = @DestAccountID;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH
```

---

## 11. Dynamic SQL

### Safe Use of Dynamic SQL

Dynamic SQL should be avoided whenever possible. When necessary, it must be implemented safely.

```sql
-- ❌ NEVER DO THIS - SQL Injection vulnerability!
DECLARE @sql NVARCHAR(MAX) = 'SELECT * FROM TB_Users WHERE UserName = ''' + @userName + '''';
EXEC(@sql);

-- ✅ Safe parameterized approach
DECLARE @sql NVARCHAR(MAX);
SET @sql = N'SELECT ID, FirstName FROM TB_Users WHERE Email = @email_param';

EXEC sp_executesql @sql, N'@email_param NVARCHAR(255)', @email_param = @userInputEmail;
```

---

## 12. WITH (NOLOCK)

### Guidelines on using WITH (NOLOCK)

The `WITH (NOLOCK)` table hint allows "dirty reads" - reading uncommitted data that might be rolled back.

**When to Use:**
```sql
-- Dashboard showing approximate counts
SELECT
    COUNT(*) AS ApproximateOrders
FROM Orders.TB_Orders WITH (NOLOCK)
WHERE OrderDate >= DATEADD(DAY, -30, GETDATE());
```

**When NOT to Use:**
```sql
-- NEVER use for financial calculations!
SELECT
    SUM(Amount) AS TotalBalance
FROM Accounts.TB_Transactions WITH (NOLOCK) -- WRONG!
WHERE AccountID = @AccountID;
```

> **Golden Rule:** NOLOCK is for "good enough" data in high-traffic scenarios. If accuracy matters, don't use it.

---

## 13. Cursors

### Discouraging Cursors

Cursors should be avoided in favor of set-based operations.

**Why Cursors are Bad:**
- **Performance:** Processing 10,000 rows means 10,000 individual operations instead of 1 set-based operation
- **Memory:** Cursors hold locks and consume memory
- **Locking:** Can cause blocking and deadlocks

```sql
-- ❌ BAD: Cursor (takes minutes)
DECLARE cursor CURSOR FOR
SELECT ID, Email
FROM dbo.TB_Applicants;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Process one row at a time
END

-- ✅ GOOD: Set-based (takes seconds)
UPDATE dbo.TB_Applicants
SET ObsoleteVerificationLevelID = @VerificationLevelID
WHERE ID = @ApplicantID
```

> **The Rule:** Before writing a cursor, document why a set-based solution is impossible. In 99% of cases, there IS a set-based solution.

---

## 14. SARGable Queries

### Writing SARGable Queries for Performance

A query is "SARGable" (Search ARGument able) if it allows the query optimizer to use an index.

```sql
-- ❌ Non-SARGable (No index use)
SELECT *
FROM Sales.TB_Orders
WHERE YEAR(OrderDate) = 2025;

-- ✅ SARGable (Uses index)
SELECT *
FROM Sales.TB_Orders
WHERE
    OrderDate >= '2025-01-01'
    AND OrderDate < '2026-01-01';
```

---

## 15. SELECT *

### Avoid SELECT * in Production Code

All SELECT statements in production code must explicitly list the columns they require.

**Reasons to Avoid SELECT *:**
- **Performance:** Retrieving unnecessary columns increases network I/O
- **Stability:** Schema changes won't break explicit column lists

---

## 16. Temporary Objects

### Guidance on Temporary Objects

| Type | Use For |
|------|---------|
| **Temporary Table (#TempTable)** | Larger datasets (over ~100 rows), when you need to create indexes, complex queries benefiting from statistics |
| **Table Variable (@TableVariable)** | Smaller datasets (less than ~100 rows), simple scenarios without indexing needs, function compatibility |

---

## 17. Schema Binding

### Using Schema Binding for Stability

The `WITH SCHEMABINDING` option locks underlying objects, preventing schema changes that would break the view or function.

```sql
CREATE VIEW Sales.VW_HighValueOrders
WITH SCHEMABINDING
AS
SELECT
    OrderID,
    OrderDate,
    CustomerID
FROM Sales.TB_Orders
WHERE OrderTotal > 1000;
```

---

# C# Coding Standards

## C# 1. Naming Conventions

### C# Naming Conventions

Consistency in naming is crucial for readability. These conventions, based on Microsoft's framework design guidelines, must be followed.

| Element | Convention | Example |
|---------|------------|---------|
| Classes, Interfaces, Enums | PascalCase | `InspectionJob`, `IHostedService` |
| Methods | PascalCase | `FillPendingApplicantSessions` |
| Properties & Public Fields | PascalCase | `Name`, `ApplicantID` |
| Private Fields | camelCase with underscore | `_connection`, `_minioClient` |
| Local Variables & Parameters | camelCase | `workerName`, `cancellationToken` |

---

## C# 2. Clean Architecture

### Project Structure Overview

Clean Architecture organizes code into layers with clear dependencies flowing inward. The inner layers contain business logic and are independent of external concerns.

```
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
```

### Layer Dependencies

```
       ┌────────────────────────────────────────────────────────────┐
       │                          Common                            │
       │        (Extensions, Constants, Shared Utilities)           │
       └──────────────────────────────┬─────────────────────────────┘
                                      │ (Used by all)
                                      ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                        Presentation (Entry Points)                       │
│      (Public API, Admin API, Workers, Controllers, Middleware)           │
└───────────────────────────┬──────────────────────────────────────────────┘
                            │                                
                            │ (References)                   
                            ▼                                
┌──────────────────────────────────────────────────────────────────────────┐
│                      Infrastructure (External)                           │
│     (EF Core Persistence, Messaging/RabbitMQ, External Clients)          │
└───────────────────────────┬──────────────────────────────────────────────┘
                            │                                
                            │ (Implements Core Interfaces)   
                            ▼                                
┌──────────────────────────────────────────────────────────────────────────┐
│                        Core: Application Layer                           │
│      (Vertical Slices, MediatR Handlers, Mappers, DTOs, v1/v2)           │
└───────────────────────────┬──────────────────────────────────────────────┘
                            │                                
                            │ (Depends on)                   
                            ▼                                
┌──────────────────────────────────────────────────────────────────────────┐
│                          Core: Domain Layer                              │
│         (Entities, Value Objects, Domain Events, Interfaces)             │
└──────────────────────────────────────────────────────────────────────────┘
```

### Clean Architecture Principles

**Domain Layer (Innermost)**
- Contains enterprise-wide business rules
- No dependencies on other layers
- Pure C# classes with no framework dependencies

```csharp
// Domain/Entities/User.cs
public class User : BaseEntity
{
    public required string Email { get; init; }
    public required string FirstName { get; private set; }
    public required string LastName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private User() { } // For EF Core
    
    public static User Create(string email, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow
        };
        
        user.AddDomainEvent(new UserCreatedEvent(user.Id));
        return user;
    }
    
    public void UpdateName(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        
        FirstName = firstName;
        LastName = lastName;
        AddDomainEvent(new UserUpdatedEvent(Id));
    }
}
```

**Application Layer**
- Contains application-specific business rules
- Orchestrates domain objects
- Defines interfaces implemented by outer layers

```csharp
// Application/Common/Interfaces/IApplicationDbContext.cs
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

**Infrastructure Layer**
- Implements interfaces defined in Application layer
- Contains EF Core, external services, file system access
- Framework-specific code lives here

**Presentation Layer**
- Entry point of the application
- Contains controllers, middleware, filters
- Minimal logic - delegates to Application layer

---

## C# 3. Formatting

### General Formatting and Style

| Rule | Description |
|------|-------------|
| **Braces** | Use the Allman style, where each brace gets its own line |
| **var Keyword** | Use `var` when the type is obvious from the right-hand side |
| **File Organization** | Each public class should reside in its own file |
| **File-Scoped Namespaces** | Use file-scoped namespaces to reduce nesting |
| **Expression-Bodied Members** | Use `=>` for simple, single-line properties and methods |

```csharp
// File-scoped namespace
namespace MyApp.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<int>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
}
```

---

## C# 4. DTOs

### Data Transfer Objects (DTOs)

DTOs are used for data transfer between layers and should be simple, immutable when possible.

| Practice | Description |
|----------|-------------|
| **Records** | Use C# records for immutable DTOs |
| **Naming** | Suffix with purpose: `UserRequest`, `UserResponse` |
| **Validation** | Use FluentValidation or data annotations |

```csharp
// Request DTO
public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName);

// Response DTO
public record UserResponse(
    int Id,
    string Email,
    string FullName,
    DateTime CreatedAt);
```

---

## C# 5. Async/Await

### Async/Await Best Practices

| Rule | Description |
|------|-------------|
| **Async All The Way** | Don't mix sync and async code; use async throughout |
| **Avoid .Result/.Wait()** | These can cause deadlocks; always await |
| **CancellationToken** | Always accept and pass CancellationToken for cancellable operations |
| **Suffix Async** | All async methods should end with `Async` suffix |
| **ValueTask** | Consider `ValueTask<T>` for hot paths where result is often synchronous |

> **Note on ConfigureAwait:** In ASP.NET Core applications, `ConfigureAwait(false)` is generally not needed because there's no synchronization context. Only use it in shared libraries that may be consumed by non-ASP.NET Core applications.

```csharp
public async Task<User> GetUserAsync(int id, CancellationToken cancellationToken)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}
```

---

## C# 6. Error Handling

### Error Handling Best Practices

| Guideline | Description |
|-----------|-------------|
| **Specific Exceptions** | Catch specific exception types, not bare `Exception` |
| **Don't Swallow** | Never catch and ignore exceptions silently |
| **Use throw;** | Rethrow with `throw;` to preserve stack trace, not `throw ex;` |
| **Log Context** | Include relevant context when logging exceptions |

```csharp
try
{
    await _userService.CreateAsync(user, cancellationToken);
}
catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
{
    _logger.LogWarning(ex, "Duplicate key violation for user {Email}", user.Email);
    throw new DuplicateEntryException("email", user.Email);
}
```

---

## C# 7. Entity Framework Core

### EF Core Configuration and Best Practices

#### DbContext Configuration

```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Apply audit fields
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTimeService.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUserService.UserId;
                    entry.Entity.ModifiedAt = _dateTimeService.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

#### Entity Configuration (Fluent API)

```csharp
// Infrastructure/Persistence/Configurations/UserConfiguration.cs
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("TB_Users", "Users");

        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id)
            .HasColumnName("ID");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetime2(7)");

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // Relationships
        builder.HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### EF Core Best Practices

**Query Optimization:**

```csharp
// ✅ Use AsNoTracking for read-only queries
var users = await _context.Users
    .AsNoTracking()
    .Where(u => u.IsActive)
    .ToListAsync(cancellationToken);

// ✅ Use projection to select only needed columns
var userDtos = await _context.Users
    .AsNoTracking()
    .Where(u => u.IsActive)
    .Select(u => new UserDto(u.Id, u.Email, u.FullName))
    .ToListAsync(cancellationToken);

// ✅ Use Include for eager loading when needed
var user = await _context.Users
    .Include(u => u.Orders)
        .ThenInclude(o => o.OrderItems)
    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

// ✅ Use AsSplitQuery for complex includes to avoid cartesian explosion
var users = await _context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .AsSplitQuery()
    .ToListAsync(cancellationToken);
```

**Avoiding Common Pitfalls:**

```csharp
// ❌ N+1 Query Problem
var users = await _context.Users.ToListAsync();
foreach (var user in users)
{
    // This triggers a query for each user!
    var orders = user.Orders.Count;
}

// ✅ Use Include or projection
var usersWithOrderCount = await _context.Users
    .Select(u => new { u.Id, u.Email, OrderCount = u.Orders.Count })
    .ToListAsync(cancellationToken);

// ❌ Loading entire entity for updates
var user = await _context.Users.FirstAsync(u => u.Id == id);
user.Email = newEmail;
await _context.SaveChangesAsync();

// ✅ Use ExecuteUpdateAsync for bulk updates (EF Core 7+)
await _context.Users
    .Where(u => u.Id == id)
    .ExecuteUpdateAsync(s => s.SetProperty(u => u.Email, newEmail), cancellationToken);
```

**Concurrency Handling:**

```csharp
// Entity with row version (EF Core 8+ recommended approach)
public class Order : BaseEntity
{
    public uint RowVersion { get; set; }
}

// Configuration using Fluent API (preferred over attributes)
builder.Property(o => o.RowVersion)
    .IsRowVersion();

// Alternative: Using byte[] for SQL Server
public class Order : BaseEntity
{
    public byte[]? RowVersion { get; set; }
}

// Configuration
builder.Property(o => o.RowVersion)
    .IsRowVersion()
    .HasConversion<byte[]>();

// Handling concurrency conflicts
try
{
    await _context.SaveChangesAsync(cancellationToken);
}
catch (DbUpdateConcurrencyException ex)
{
    // Get the affected entries
    foreach (var entry in ex.Entries)
    {
        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
        if (databaseValues is null)
        {
            throw new NotFoundException(entry.Entity.GetType().Name.ToLower());
        }
        
        // Client wins: entry.OriginalValues.SetValues(databaseValues);
        // Database wins: entry.CurrentValues.SetValues(databaseValues);
    }
    
    throw new ConcurrencyConflictException("order");
}
```

**Interceptors:**

```csharp
// Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = _currentUserService.UserId;
                entry.Entity.CreatedAt = _dateTimeService.UtcNow;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.ModifiedBy = _currentUserService.UserId;
                entry.Entity.ModifiedAt = _dateTimeService.UtcNow;
            }
        }
    }
}
```

**Registration:**

```csharp
// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Register interceptors
    services.AddScoped<AuditableEntityInterceptor>();
    services.AddScoped<DispatchDomainEventsInterceptor>();

    // Use DbContext pooling for better performance
    services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
    {
        options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
                
                // Enable for query performance insights
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        
        // Add interceptors from DI
        var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
        var domainEventsInterceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();
        options.AddInterceptors(auditInterceptor, domainEventsInterceptor);
        
        // Enable detailed errors in development only
        #if DEBUG
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        #endif
    });

    services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<ApplicationDbContext>());

    return services;
}
```

> **Note:** `AddDbContextPool` provides better performance than `AddDbContext` for high-throughput scenarios. However, avoid capturing scoped services in the DbContext constructor when using pooling.

---

## C# 8. MediatR

### MediatR Pattern Implementation

MediatR implements the Mediator pattern, decoupling request handling from controllers.

#### Command/Query Separation (CQRS)

```csharp
// Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs
public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName) : IRequest<int>;

// Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(request.Email, request.FirstName, request.LastName);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created with ID {UserId}", user.Id);

        return user.Id;
    }
}
```

#### Queries

```csharp
// Application/Features/Users/Queries/GetUserById/GetUserByIdQuery.cs
public record GetUserByIdQuery(int Id) : IRequest<UserResponse?>;

// Application/Features/Users/Queries/GetUserById/GetUserByIdQueryHandler.cs
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserMapper _mapper;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        IUserMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserResponse?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? null : _mapper.ToResponse(user);
    }
}
```

#### Pipeline Behaviors

```csharp
// Application/Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}

// Application/Common/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;

        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}", 
            requestName, 
            userId);

        var response = await next();

        _logger.LogInformation(
            "Handled {RequestName} for user {UserId}", 
            requestName, 
            userId);

        return response;
    }
}

// Application/Common/Behaviors/PerformanceBehavior.cs
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;

    public PerformanceBehavior(ILogger<TRequest> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();
        var response = await next();
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds}ms) {@Request}",
                requestName, elapsedMilliseconds, request);
        }

        return response;
    }
}
```

#### FluentValidation Integration

```csharp
// Application/Features/Users/Commands/CreateUser/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("email_required")
            .EmailAddress().WithMessage("email_invalid_format")
            .MaximumLength(255).WithMessage("email_max_length")
            .MustAsync(BeUniqueEmail).WithMessage("email_already_exists");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("first_name_required")
            .MaximumLength(100).WithMessage("first_name_max_length");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("last_name_required")
            .MaximumLength(100).WithMessage("last_name_max_length");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
```

#### Registration

```csharp
// Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // MediatR 12.x registration
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        
        // Pipeline behaviors are registered in order of execution
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    });

    // FluentValidation - register all validators from assembly
    services.AddValidatorsFromAssembly(
        typeof(DependencyInjection).Assembly,
        includeInternalTypes: true);

    return services;
}
```

> **Note:** MediatR 12.x uses `AddMediatR` with configuration action. The older `AddMediatR(Assembly)` syntax is deprecated.

#### Controller Usage

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
```

---

## C# 9. API Versioning

### URL Path Versioning Strategy

API versioning via URL path is the standard approach. All API routes must include the version in the path.

#### Route Format

```
/api/v{major}/resource
/api/v{major}.{minor}/resource (when minor version needed)
```

**Examples:**
```
GET  /api/v1/users
GET  /api/v1/users/123
POST /api/v1/users
GET  /api/v2/users          (new version with breaking changes)
```

#### Configuration

```csharp
// Program.cs or Presentation/DependencyInjection.cs
public static IServiceCollection AddPresentation(this IServiceCollection services)
{
    services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    return services;
}
```

#### Controller Implementation

```csharp
// V1 Controller
[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseV1>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseV1>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }
}

// V2 Controller (with breaking changes)
[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("2.0")]
public class UsersV2Controller : ControllerBase
{
    private readonly ISender _mediator;

    public UsersV2Controller(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseV2>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQueryV2(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // New endpoint only in V2
    [HttpGet("{id}/profile")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserProfileQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
```

#### Deprecating API Versions

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0", Deprecated = true)]  // Mark as deprecated
public class UsersController : ControllerBase
{
    // V1 endpoints will include deprecation warning in response headers
}
```

#### Project Structure for Versioned APIs

```
src/
├── 0-Common/                        # Shared by all layers
│   ├── Extensions/                  # StringExtensions, ServiceCollectionExtensions
│   └── Constants/
│
├── 1-Core/
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
├── 2-Infrastructure/                # External Concerns
│   ├── Persistence/                 # EF Core, Migrations, Interceptors
│   ├── Messaging/                   # Message Broker (RabbitMQ/MassTransit)
│   ├── ExternalClients/             # 3rd Party API Clients
│   └── DependencyInjection.cs
│
├── 3-Presentation/                  # Entry Points (Multiple .csproj)
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
```


## C# 10. Mapperly

### Mapperly Configuration and Best Practices

Mapperly is a source generator for object mapping, providing compile-time safety and excellent performance.

#### Basic Mapper Definition

```csharp
// Application/Common/Mappings/UserMapper.cs
[Mapper]
public partial class UserMapper
{
    // Simple mapping (properties with same name)
    public partial UserResponse ToResponse(User user);
    
    // Collection mapping (auto-generated, no need to define explicitly)
    public partial IReadOnlyList<UserResponse> ToResponseList(IEnumerable<User> users);
    
    // Ignore specific source property
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    public partial UserDto ToDto(User user);
    
    // Ignore specific target property
    [MapperIgnoreTarget(nameof(UserResponse.TempField))]
    public partial UserResponse ToResponseIgnoreTemp(User user);
}
```

#### Advanced Mapping Scenarios

```csharp
[Mapper]
public partial class OrderMapper
{
    // Nested object mapping - automatically handled
    public partial OrderResponse ToResponse(Order order);
    
    // Flattening with MapProperty
    [MapProperty(nameof(Order.Customer), nameof(Order.Customer.Name), nameof(OrderResponse.CustomerName))]
    public partial OrderResponse ToFlatResponse(Order order);
    
    // Custom value conversion using UserMapping
    [UserMapping(Default = true)]
    private string MapStatus(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Pending",
        OrderStatus.Processing => "In Progress",
        OrderStatus.Completed => "Done",
        OrderStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
    
    // The mapper will automatically use MapStatus for OrderStatus -> string
    public partial OrderResponse ToResponseWithStatus(Order order);
    
    // Mapping with runtime parameters
    [MapperIgnoreSource(nameof(baseUrl))]
    public partial UserResponse ToResponse(User user, string baseUrl);
    
    // After map hook for additional logic
    [AfterMap]
    private static void AfterMap(Order source, OrderResponse target)
    {
        target.DisplayName = $"{source.OrderNumber} - {source.Customer?.Name}";
    }
}
```

#### Mapper Configuration

```csharp
// Global mapper configuration using MapperDefaults (Mapperly 4.x)
[Mapper(
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = true,
    RequiredMappingStrategy = RequiredMappingStrategy.Target,
    UseDeepCloning = false)]
public partial class AppMapper
{
    // All mapping methods inherit these settings
}

// Strict mapper - all properties must be explicitly mapped
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public partial class StrictUserMapper
{
    public partial UserResponse ToResponse(User user);
}
```

#### Registration and Usage

```csharp
// Application/DependencyInjection.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Mapperly mappers are stateless - register as Singleton
    services.AddSingleton<UserMapper>();
    services.AddSingleton<OrderMapper>();
    
    return services;
}

// Usage in handler - inject mapper directly (no interface needed for source-generated mappers)
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IApplicationDbContext _context;
    private readonly UserMapper _mapper;

    public GetUserByIdQueryHandler(IApplicationDbContext context, UserMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserResponse?> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? null : _mapper.ToResponse(user);
    }
}
```

#### Projection Mapping (EF Core Integration)

```csharp
[Mapper]
public static partial class UserProjectionMapper
{
    // Use for EF Core projections - generates expression tree
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial IQueryable<UserResponse> ProjectToResponse(this IQueryable<User> query);
}

// Usage - translates to SQL
var users = await _context.Users
    .Where(u => u.IsActive)
    .ProjectToResponse()
    .ToListAsync(cancellationToken);
```

#### Mapperly Best Practices

| Practice | Description |
|----------|-------------|
| **Source Generation** | Mapperly generates code at compile time - no runtime reflection overhead |
| **Stateless Mappers** | Register mappers as Singleton - they have no state |
| **Compile-Time Validation** | Fix all warnings - Mapperly validates mappings at compile time |
| **Use Projections** | For EF Core queries, use `ProjectTo` methods to generate SQL-translatable expressions |
| **Explicit Ignore** | Use `[MapperIgnoreSource]` and `[MapperIgnoreTarget]` instead of leaving unmapped properties |
| **UserMapping** | Use `[UserMapping]` for custom type conversions that should be reused |

---

## C# 11. Configuration

### Configuration Management

```csharp
// Options class pattern
public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
}

public class GraylogOptions
{
    public const string SectionName = "Graylog";
    
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 12201;
    public string Transport { get; set; } = "Udp";
    public string Facility { get; set; } = string.Empty;
}

// Registration
services.Configure<DatabaseOptions>(
    configuration.GetSection(DatabaseOptions.SectionName));
services.Configure<GraylogOptions>(
    configuration.GetSection(GraylogOptions.SectionName));

// Usage
public class MyService
{
    private readonly DatabaseOptions _options;
    
    public MyService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }
}
```

---

## C# 12. Best Practices

### General Best Practices

| Guideline | Description |
|-----------|-------------|
| **DRY Principle** | Don't Repeat Yourself - extract common code into methods |
| **KISS** | Keep It Simple, Stupid - favor simplicity over cleverness |
| **Null Checks** | Use null-conditional operators and pattern matching |
| **Dependency Injection** | Always use DI for dependencies, avoid `new` for services |

---

## C# 13. Documentation

### XML Documentation

```csharp
/// <summary>
/// Creates a new user in the system.
/// </summary>
/// <param name="user">The user to create.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created user with assigned ID.</returns>
/// <exception cref="ValidationException">Thrown when user data is invalid.</exception>
/// <exception cref="DuplicateEntryException">Thrown when email already exists.</exception>
public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken)
{
    // Implementation
}
```

### When to Comment

- Complex business logic that isn't self-evident
- Workarounds with ticket references
- Public APIs and interfaces
- Non-obvious performance optimizations

---

## C# 14. DI Scope Rules

### Service Lifetimes

| Lifetime | When to Use | Examples |
|----------|-------------|----------|
| **Singleton** | Stateless services, caches, configuration | `IMemoryCache`, `IMapper` |
| **Scoped** | Per-request state, database contexts | `DbContext`, `ICurrentUserService` |
| **Transient** | Lightweight, stateless operations | `ISmsService`, `IEmailService` |

```csharp
// ❌ Never inject Scoped into Singleton
public class CacheService  // Singleton
{
    private readonly ApplicationDbContext _context; // BAD: Scoped in Singleton
}

// ✅ Use IServiceScopeFactory
public class CacheService  // Singleton
{
    private readonly IServiceScopeFactory _factory;

    public async Task RefreshAsync()
    {
        using var scope = _factory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}
```

---

## C# 15. Serilog & Logging Standards

### Serilog Configuration

```csharp
// Program.cs - Modern .NET 8+ approach
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog using the builder
builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Graylog(new GraylogSinkOptions
        {
            HostnameOrAddress = builder.Configuration["Graylog:Host"] ?? "localhost",
            Port = builder.Configuration.GetValue("Graylog:Port", 12201),
            TransportType = Enum.Parse<TransportType>(
                builder.Configuration["Graylog:Transport"] ?? "Udp"),
            Facility = builder.Configuration["Graylog:Facility"] ?? builder.Environment.ApplicationName
        });
});

var app = builder.Build();

// Add request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
    
    // Don't log health checks
    options.GetLevel = (httpContext, elapsed, ex) =>
        httpContext.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose
            : LogEventLevel.Information;
});
```

> **Note:** The `builder.Host.UseSerilog()` approach is being phased out in favor of `builder.Services.AddSerilog()`.
```

### appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  },
  "Graylog": {
    "Host": "graylog.internal.company.com",
    "Port": 12201,
    "Transport": "Udp",
    "Facility": "MyApp"
  }
}
```

### Structured Logging Best Practices

```csharp
// ❌ BAD - Don't use Console
Console.WriteLine("Processing started");
Console.WriteLine($"User {userId} logged in");

// ❌ BAD - String interpolation (no structured data, allocates even when log level disabled)
_logger.LogInformation($"User {userId} logged in");

// ✅ GOOD - Structured logging with placeholders
_logger.LogInformation("Processing started");
_logger.LogInformation("User {UserId} logged in", userId);
_logger.LogError(ex, "Failed to process user {UserId}", userId);

// ✅ BETTER - High-performance logging with source generators (.NET 8+)
public static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} logged in")]
    public static partial void UserLoggedIn(ILogger logger, int userId);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "User {UserId} failed login attempt {AttemptNumber}")]
    public static partial void LoginFailed(ILogger logger, int userId, int attemptNumber);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process user {UserId}")]
    public static partial void ProcessingFailed(ILogger logger, Exception ex, int userId);
}

// Usage
LogMessages.UserLoggedIn(_logger, userId);
LogMessages.ProcessingFailed(_logger, ex, userId);
```

### Proper Logger Implementation

```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessUserAsync(int userId)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["Operation"] = "ProcessUser"
        }))
        {
            _logger.LogInformation("Processing user started");

            try
            {
                // Business logic
                _logger.LogDebug("User validation passed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process user");
                throw;
            }
        }
    }
}
```

### Log Levels Guide

| Level | Usage |
|-------|-------|
| **Trace** | Very detailed diagnostic info (disabled in production) |
| **Debug** | Debugging information for development |
| **Information** | General operational events (request started, completed) |
| **Warning** | Unexpected events that don't stop execution |
| **Error** | Errors and exceptions that need attention |
| **Critical** | System failures requiring immediate action |

> **Important:** Always use structured logging with placeholders like `{UserId}` instead of string interpolation. This enables better log querying and analysis.

---

## C# 16. Graylog Integration

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│  ENVIRONMENT VARIABLES                                          │
│  GRAYLOG_HOST=graylog.internal.company.com                     │
│  GRAYLOG_PORT=12201                                            │
│  GRAYLOG_TRANSPORT=Udp                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Application                                                     │
│      Serilog → Graylog Sink → GELF Protocol                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Graylog Server                                                  │
│  - Centralized logging                                           │
│  - Full-text search                                              │
│  - Dashboards & Alerts                                           │
└─────────────────────────────────────────────────────────────────┘
```

### Searching Logs in Graylog

```bash
# Find all errors for an application
Application:"Verification.API" AND Level:"Error"

# Find by TraceId (from error response)
TraceId:"0HN4DVGL9SK2T"

# Find by environment
Environment:"production" AND Application:"MyApp.API"

# Find validation errors
Message:"validation_error"

# Find errors in last hour
Application:"MyApp.API" AND Level:"Error" AND timestamp:[now-1h TO now]
```

### Customer Support Flow

1. Customer reports: "I got an error when creating an applicant"
2. Support asks: "What's the error traceId?"
3. Customer: "0HN4DVGL9SK2T"
4. Support searches Graylog: `TraceId:"0HN4DVGL9SK2T"`
5. Support identifies the issue from the detailed logs

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `GRAYLOG_HOST` | Required | Graylog server address |
| `GRAYLOG_PORT` | 12201 | GELF port |
| `GRAYLOG_TRANSPORT` | Udp | Udp / Tcp / Http |
| `GRAYLOG_FACILITY` | App Name | Facility name |
| `LOG_LEVEL_DEFAULT` | Information | Default log level |
| `LOG_LEVEL_MICROSOFT` | Warning | Microsoft.* namespace level |

> **Important:** Never log sensitive data (passwords, tokens, personal data). Use structured logging with placeholders.

---

## C# 17. Background Jobs

### Background Jobs with Hangfire

```csharp
public interface IJob
{
    string Name { get; }
    string Expression { get; }  // Cron expression
    bool RunImmediately { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public class NotificationsSenderJob : IJob
{
    public string Name => "NotificationsSenderJob";
    public string Expression => "*/10 * * * *";  // Every 10 minutes
    public bool RunImmediately => true;

    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationsSenderJob> _logger;

    public NotificationsSenderJob(
        ISmsService smsService,
        IEmailService emailService,
        ILogger<NotificationsSenderJob> logger)
    {
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotificationsSenderJob started");

        try
        {
            // Job logic here
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotificationsSenderJob failed");
            throw;
        }
    }
}
```

### Common Cron Expressions

| Expression | Description |
|------------|-------------|
| `* * * * *` | Every minute |
| `*/5 * * * *` | Every 5 minutes |
| `0 * * * *` | Every hour at minute 0 |
| `0 0 * * *` | Daily at midnight |
| `0 0 * * 0` | Weekly on Sunday |

---

## C# 18. EMQX/MQTT

### MQTT Messaging Patterns

```csharp
public class NotificationService
{
    private readonly IPublisher _publisher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IPublisher publisher, ILogger<NotificationService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task SendUserNotificationAsync(Guid userGuid, object notification)
    {
        var topic = $"UserNotifications/{userGuid}";
        var payload = JsonSerializer.Serialize(notification);

        var result = await _publisher.PublishAsync(
            topic: topic,
            payload: payload,
            retain: false,
            qualityLevel: PublishQualityLevel.AtLeastOnce
        );

        if (!result.IsSuccess)
            _logger.LogError("Failed to publish to {Topic}: {Error}", topic, result.Error);
    }
}
```

### Quality of Service (QoS) Levels

| Level | Description |
|-------|-------------|
| **AtMostOnce (0)** | Fire and forget, no guarantee |
| **AtLeastOnce (1)** | Guaranteed delivery, may duplicate (recommended) |
| **ExactlyOnce (2)** | Guaranteed exactly once, highest overhead |

---

## C# 19. Authorization

### HMAC Signature Authorization

```csharp
private string CreateSignature(
    long timestamp,
    HttpMethod httpMethod,
    string path,
    string secretKey,
    byte[] body)
{
    var message = $"{timestamp}{httpMethod.Method}{path}";
    var messageBytes = Encoding.ASCII.GetBytes(message);

    using var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secretKey));
    var hash = hmac.ComputeHash(messageBytes);

    return hash.Aggregate("", (s, e) => s + $"{e:x2}");
}
```

---

## C# 20. Request Validation

### FluentValidation with MediatR

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("email_required")
            .EmailAddress().WithMessage("email_invalid_format")
            .MaximumLength(255).WithMessage("email_max_length")
            .MustAsync(BeUniqueEmail).WithMessage("email_already_exists");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("first_name_required")
            .MaximumLength(100).WithMessage("first_name_max_length");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("last_name_required")
            .MaximumLength(100).WithMessage("last_name_max_length");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120).WithMessage("age_out_of_range");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
```

### Error Message Convention

- Use `snake_case`: `email_required`, `user_not_found`
- Be specific: `password_too_short` instead of `invalid_password`
- Include field name: `first_name_required`, `email_invalid`

---

## C# 21. Error Responses (RFC 7807)

### Example Error Response

```json
{
  "type": "validation_error",
  "title": "Validation Error",
  "status": 400,
  "code": "email_invalid_format",
  "detail": "Validation failed for field 'email': invalid value",
  "instance": "/api/users/create",
  "traceId": "0HN4DVGL9SK2T:00000001",
  "timestamp": "2025-01-15T14:32:00Z",
  "errors": [
    {
      "field": "email",
      "code": "invalid_format",
      "rejectedValue": "not-an-email"
    }
  ]
}
```

### Response Fields Reference

| Field | Purpose | Example |
|-------|---------|---------|
| `type` | Error category for grouping | `not_found`, `validation_error` |
| `title` | Human-readable title | "Not Found", "Validation Error" |
| `status` | HTTP status code | 400, 404, 500 |
| `code` | Translation key | `user_not_found`, `email_required` |
| `detail` | Human-readable message | "The requested user was not found" |
| `instance` | Request URL | "/api/users/create" |
| `traceId` | Correlation ID for logs | "0HN4DVGL9SK2T" |
| `errors` | Field-level validation errors | Array of field errors |

---

## C# 22. Custom Exceptions

### Exception Hierarchy

```csharp
// Base Exception
public class DomainException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public string Type { get; }

    public DomainException(string type, string code, string message, int statusCode)
        : base(message)
    {
        Type = type;
        Code = code;
        StatusCode = statusCode;
    }
}

// 400 Bad Request
public class ValidationException : DomainException
{
    public IReadOnlyList<FieldError> Errors { get; }

    public ValidationException(string field, string code)
        : base("validation_error", $"{field}_{code}", 
               $"Validation failed for field '{field}'", 400)
    {
        Errors = new[] { new FieldError(field, code) };
    }

    public ValidationException(IEnumerable<FieldError> errors)
        : base("validation_error", "validation_failed",
               $"{errors.Count()} validation error(s) occurred", 400)
    {
        Errors = errors.ToList();
    }
}

// 404 Not Found
public class NotFoundException : DomainException
{
    public NotFoundException(string resource)
        : base("not_found", $"{resource}_not_found",
               $"The requested {resource} was not found", 404)
    { }
}

// 409 Conflict
public class DuplicateEntryException : DomainException
{
    public DuplicateEntryException(string field, object value)
        : base("duplicate_entry", $"{field}_duplicate",
               $"A record with this {field} already exists", 409)
    { }
}

// 422 Business Rule Violation
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base("business_rule_violation", code, message, 422)
    { }
}
```

### Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Type = validationEx.Type,
                Title = "Validation Error",
                Status = validationEx.StatusCode,
                Detail = validationEx.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["code"] = validationEx.Code,
                    ["traceId"] = traceId,
                    ["errors"] = validationEx.Errors
                }
            },
            DomainException domainEx => new ProblemDetails
            {
                Type = domainEx.Type,
                Title = GetTitle(domainEx.StatusCode),
                Status = domainEx.StatusCode,
                Detail = domainEx.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["code"] = domainEx.Code,
                    ["traceId"] = traceId
                }
            },
            _ => new ProblemDetails
            {
                Type = "internal_error",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["code"] = "internal_server_error",
                    ["traceId"] = traceId
                }
            }
        };

        _logger.LogError(exception,
            "Error processing request. TraceId: {TraceId}, Type: {Type}, Code: {Code}",
            traceId, problemDetails.Type, problemDetails.Extensions["code"]);

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        _ => "Internal Server Error"
    };
}
```

---

## C# 23. Error Handling Best Practices

### Security: Never Expose Internal Details

**What NOT to expose in production:**
- SQL queries, table names, column names
- Internal service URLs
- Stack traces with file paths
- Connection strings or credentials

### Use Specific Exception Types

```csharp
// ❌ Bad: Generic exceptions
throw new Exception("User not found");

// ✅ Good: Specific exceptions
throw new NotFoundException("user");
throw new DuplicateEntryException("email", value);
throw new ValidationException("email", "required");
```

### Validation Best Practices

```csharp
// ❌ Bad: Fail on first error
if (string.IsNullOrEmpty(req.Email))
    throw new ValidationException("email", "required");

// ✅ Good: Collect all errors
var errors = new List<FieldError>();
if (string.IsNullOrEmpty(req.Email))
    errors.Add(new FieldError("email", "required"));
if (string.IsNullOrEmpty(req.Name))
    errors.Add(new FieldError("name", "required"));
if (errors.Any())
    throw new ValidationException(errors);
```

---

## C# 24. Unit Testing

### Testing Best Practices for Clean Architecture

#### Project Structure

```
tests/
├── Domain.UnitTests/
│   └── Entities/
├── Application.UnitTests/
│   ├── Features/
│   │   └── Users/
│   │       └── Commands/
│   └── Common/
│       └── Behaviors/
├── Presentation.UnitTests/
│   └── Controllers/
├── WebApi.IntegrationTests/
│   └── Controllers/
└── Architecture.Tests/
    └── DependencyTests.cs
```

> **Testing Philosophy:** Focus unit tests on business logic (Domain entities, Application handlers, validators, behaviors) and Controllers. Do NOT unit test the Infrastructure layer (repositories, DbContext) - these are implementation details tested through integration tests.

#### Testing Frameworks and Libraries

| Package | Purpose |
|---------|---------|
| **xUnit** | Test framework (preferred for .NET) |
| **FluentAssertions** | Readable assertions |
| **NSubstitute** | Mocking framework |
| **Bogus** | Fake data generation |
| **Verify** | Snapshot testing |
| **Microsoft.AspNetCore.Mvc.Testing** | Integration testing for Web API |
| **Testcontainers** | Real database for integration tests |
| **Respawn** | Database reset between integration tests |

> **Important:** Do NOT unit test repositories or DbContext directly. Test business logic through handlers with mocked `IApplicationDbContext`. Use integration tests with real database for end-to-end verification.

#### Testing MediatR Handlers (Business Logic)

```csharp
public class CreateUserCommandHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly UserMapper _mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        // Mock the DbContext interface - don't test EF Core itself
        _context = Substitute.For<IApplicationDbContext>();
        _mapper = new UserMapper();
        _logger = Substitute.For<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_context, _mapper, _logger);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesUserAndReturnsId()
    {
        // Arrange
        var command = new CreateUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe");

        _context.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        _context.Users.Received(1).Add(Arg.Is<User>(u => 
            u.Email == "test@example.com" && 
            u.FirstName == "John"));
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_LogsUserCreation()
    {
        // Arrange
        var command = new CreateUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("User created")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
```

#### Testing Validators

```csharp
public class CreateUserCommandValidatorTests
{
    private readonly IApplicationDbContext _context;
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _validator = new CreateUserCommandValidator(_context);
    }

    [Fact]
    public async Task Validate_ValidCommand_ReturnsNoErrors()
    {
        // Arrange
        var command = new CreateUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe");

        _context.Users.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "email_required")]
    [InlineData("invalid-email", "email_invalid_format")]
    public async Task Validate_InvalidEmail_ReturnsError(string email, string expectedError)
    {
        // Arrange
        var command = new CreateUserCommand(
            Email: email,
            FirstName: "John",
            LastName: "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Fact]
    public async Task Validate_DuplicateEmail_ReturnsError()
    {
        // Arrange
        var command = new CreateUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe");

        // Mock: email already exists
        _context.Users.AnyAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "email_already_exists");
    }
}
```

#### Testing Domain Entities

```csharp
public class UserTests
{
    [Fact]
    public void Create_ValidInput_ReturnsUserWithDomainEvent()
    {
        // Act
        var user = User.Create("test@example.com", "John", "Doe");

        // Assert
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedEvent>();
    }

    [Fact]
    public void UpdateName_ValidInput_UpdatesNameAndRaisesEvent()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe");
        user.ClearDomainEvents();

        // Act
        user.UpdateName("Jane", "Smith");

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserUpdatedEvent>();
    }
}
```

#### Testing Pipeline Behaviors

```csharp
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest();
        var nextCalled = false;

        // Act
        await behavior.Handle(
            request,
            () => { nextCalled = true; return Task.FromResult(new TestResponse()); },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Field", "Error") }));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(new[] { validator });

        // Act
        var act = () => behavior.Handle(
            new TestRequest(),
            () => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    private record TestRequest : IRequest<TestResponse>;
    private record TestResponse;
}
```

#### Testing Controllers

```csharp
public class UsersControllerTests
{
    private readonly ISender _mediator;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mediator = Substitute.For<ISender>();
        _controller = new UsersController(_mediator);
    }

    [Fact]
    public async Task GetById_UserExists_ReturnsOkWithUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new UserResponse(userId, "test@example.com", "John Doe", DateTime.UtcNow);
        
        _mediator.Send(Arg.Is<GetUserByIdQuery>(q => q.Id == userId), Arg.Any<CancellationToken>())
            .Returns(expectedUser);

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var user = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        user.Id.Should().Be(userId);
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetById_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        
        _mediator.Send(Arg.Is<GetUserByIdQuery>(q => q.Id == userId), Arg.Any<CancellationToken>())
            .Returns((UserResponse?)null);

        // Act
        var result = await _controller.GetById(userId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ValidCommand_ReturnsCreatedAtAction()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "Doe");
        var createdUserId = 42;
        
        _mediator.Send(command, Arg.Any<CancellationToken>())
            .Returns(createdUserId);

        // Act
        var result = await _controller.Create(command, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UsersController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdUserId);
        createdResult.Value.Should().Be(createdUserId);
    }

    [Fact]
    public async Task Create_SendsCommandToMediator()
    {
        // Arrange
        var command = new CreateUserCommand("test@example.com", "John", "Doe");
        _mediator.Send(command, Arg.Any<CancellationToken>()).Returns(1);

        // Act
        await _controller.Create(command, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateUserCommand>(c => 
                c.Email == "test@example.com" && 
                c.FirstName == "John" && 
                c.LastName == "Doe"),
            Arg.Any<CancellationToken>());
    }
}
```

#### Architecture Tests with ArchUnitNET

```csharp
public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Domain.AssemblyReference).Assembly,
            typeof(Application.AssemblyReference).Assembly,
            typeof(Infrastructure.AssemblyReference).Assembly,
            typeof(Presentation.AssemblyReference).Assembly)
        .Build();

    private readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInAssembly(typeof(Domain.AssemblyReference).Assembly);

    private readonly IObjectProvider<IType> _applicationLayer =
        Types().That().ResideInAssembly(typeof(Application.AssemblyReference).Assembly);

    private readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInAssembly(typeof(Infrastructure.AssemblyReference).Assembly);

    [Fact]
    public void Domain_ShouldNot_DependOnOtherLayers()
    {
        Types()
            .That().Are(_domainLayer)
            .Should().NotDependOnAny(_applicationLayer)
            .AndShould().NotDependOnAny(_infrastructureLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Application_ShouldNot_DependOnInfrastructure()
    {
        Types()
            .That().Are(_applicationLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Handlers_Should_HaveCorrectNaming()
    {
        Classes()
            .That().ImplementInterface(typeof(IRequestHandler<,>))
            .Should().HaveNameEndingWith("Handler")
            .Check(Architecture);
    }
}
```

#### Test Data Builders with Bogus

```csharp
public class UserFaker : Faker<User>
{
    public UserFaker()
    {
        CustomInstantiator(f => User.Create(
            f.Internet.Email(),
            f.Name.FirstName(),
            f.Name.LastName()));
    }

    public UserFaker WithEmail(string email)
    {
        CustomInstantiator(f => User.Create(
            email,
            f.Name.FirstName(),
            f.Name.LastName()));
        return this;
    }
}

// Usage
[Fact]
public async Task GetAllUsers_ReturnsAllUsers()
{
    // Arrange
    var faker = new UserFaker();
    var users = faker.Generate(10);
    _context.Users.AddRange(users);
    await _context.SaveChangesAsync();

    // Act
    var result = await _handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

    // Assert
    result.Should().HaveCount(10);
}
```

#### Integration Testing with WebApplicationFactory

```csharp
// Custom WebApplicationFactory for integration tests
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Strong_Password_123!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add DbContext with test database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

// Base class for API integration tests
public abstract class ApiIntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = null!;

    protected ApiIntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Run migrations
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        
        // Setup Respawn
        var connectionString = context.Database.GetConnectionString()!;
        _respawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions
        {
            TablesToIgnore = new[] { "__EFMigrationsHistory" }
        });
    }

    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await _respawner.ResetAsync(context.Database.GetConnectionString()!);
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }
}

// API integration tests
public class UsersApiIntegrationTests : ApiIntegrationTestBase
{
    private const string BaseUrl = "/api/v1/users";
    
    public UsersApiIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedWithLocation()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "John", "Doe");

        // Act
        var response = await PostAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/v1/users/");
        
        var userId = await response.Content.ReadFromJsonAsync<int>();
        userId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new CreateUserRequest("duplicate@example.com", "John", "Doe");
        await PostAsync(BaseUrl, request); // Create first user

        // Act
        var response = await PostAsync(BaseUrl, request); // Try duplicate

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails!.Extensions["code"].ToString().Should().Be("email_duplicate");
    }

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {
        // Arrange
        var createRequest = new CreateUserRequest("get-test@example.com", "John", "Doe");
        var createResponse = await PostAsync(BaseUrl, createRequest);
        var userId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var user = await GetAsync<UserResponse>($"{BaseUrl}/{userId}");

        // Assert
        user.Should().NotBeNull();
        user!.Email.Should().Be("get-test@example.com");
        user.FullName.Should().Contain("John");
    }

    [Fact]
    public async Task GetUser_NonExistentUser_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"{BaseUrl}/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest("invalid-email", "John", "Doe");

        // Act
        var response = await PostAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails!.Extensions["errors"].Should().NotBeNull();
    }
}
```

### Testing Best Practices Summary

| Practice | Description |
|----------|-------------|
| **Arrange-Act-Assert** | Structure tests clearly with these three sections |
| **One Concept Per Test** | Keep tests focused on a single behavior (multiple asserts OK if testing one concept) |
| **Descriptive Names** | Use `MethodName_Scenario_ExpectedResult` or Given/When/Then naming |
| **Test Isolation** | Each test should be independent - use `IAsyncLifetime` for setup/teardown |
| **Mock Dependencies** | Mock `IApplicationDbContext` and other interfaces in unit tests |
| **Don't Test Infrastructure** | Don't unit test repositories/DbContext - use integration tests instead |
| **Integration Tests for APIs** | Use `WebApplicationFactory` with real database for end-to-end testing |
| **Use Respawn** | Fast database cleanup between integration tests |
| **Test Business Logic** | Focus on handlers, validators, domain entities, and controllers |
| **Architecture Tests** | Enforce architectural rules with ArchUnitNET |

---

## C# 25. Observability

### Logging & Monitoring Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Application                                                     │
│      Serilog → Graylog Sink                                     │
│      OpenTelemetry → Tracing                                    │
└─────────────────────────────────────────────────────────────────┘
                              │
               ┌──────────────┴──────────────┐
               ▼                              ▼
        ┌────────────┐                 ┌────────────┐
        │  Graylog   │                 │  Datadog   │
        │  ────────  │                 │  ────────  │
        │  Logs      │                 │  APM       │
        │  Search    │                 │  Traces    │
        │  Alerts    │                 │  Metrics   │
        └────────────┘                 └────────────┘
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `GRAYLOG_HOST` | Required | Graylog server address |
| `GRAYLOG_PORT` | 12201 | GELF port |
| `LOG_LEVEL_DEFAULT` | Information | Default log level |
| `LOG_LEVEL_MICROSOFT` | Warning | Microsoft namespace level |
| `INCLUDE_DEBUG_INFO` | false | Include stack traces (dev only) |

---

## Quick Reference

### Exception Type Selection

| Scenario | Exception | Result Code |
|----------|-----------|-------------|
| User not found | `NotFoundException("user")` | `user_not_found` |
| Email is required | `ValidationException("email", "required")` | `email_required` |
| Email already exists | `DuplicateEntryException("email", val)` | `email_duplicate` |
| Insufficient balance | `BusinessRuleException("insufficient_balance", "...")` | `insufficient_balance` |
| Token expired | `TokenExpiredException()` | `token_expired` |

### Clean Architecture Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|----------------|--------------|
| **Domain** | Entities, Value Objects, Domain Events | None |
| **Application** | Use Cases, DTOs, Interfaces | Domain |
| **Infrastructure** | EF Core, External Services, Logging | Application, Domain |
| **Presentation** | Controllers, Middleware, Filters | Application |

### Testing Checklist

- [ ] Unit tests for MediatR handlers (with mocked DbContext)
- [ ] Unit tests for validators
- [ ] Unit tests for domain entities
- [ ] Unit tests for pipeline behaviors
- [ ] Unit tests for controllers (with mocked MediatR)
- [ ] Integration tests for API endpoints (with real database)
- [ ] Architecture tests for dependency rules
- [ ] **Do NOT** unit test repositories or DbContext directly
