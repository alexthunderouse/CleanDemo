# Coding Standards

SQL Server & C# coding standards and best practices

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

1. [Naming Conventions](#1-naming-conventions-1)
2. [Architecture](#2-architecture)
3. [Formatting](#3-formatting)
4. [DTOs](#4-dtos)
5. [Async/Await](#5-asyncawait)
6. [Error Handling](#6-error-handling-1)
7. [Configuration](#7-configuration)
8. [Best Practices](#8-best-practices)
9. [Documentation](#9-documentation-1)
10. [DI Scope Rules](#10-di-scope-rules)
11. [Logging Standards](#11-logging-standards)
12. [Background Jobs](#12-background-jobs)
13. [EMQX/MQTT](#13-emqxmqtt)
14. [Authorization](#14-authorization)
15. [Configuration](#15-configuration-1)
16. [Request Validation](#16-request-validation)
17. [Error Responses (RFC 7807)](#17-error-responses-rfc-7807)
18. [Custom Exceptions](#18-custom-exceptions)
19. [Error Handling Best Practices](#19-error-handling-best-practices)
20. [Observability](#20-observability)

---

**Standards Info**
- SQL Sections: 17
- C# Sections: 20
- Last Updated: 2025-01

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

### Naming Rules

- **Prefix:** All table names must begin with the prefix `TB_`
- **Casing:** Use UpperCamelCase (e.g., `TB_ApplicantSessions`)
- **Clarity:** Names should be descriptive, unambiguous, and in plural form where appropriate
- **Junction Tables:** For many-to-many relationships, combine the two entities with the primary entity first (e.g., `TB_ApplicantInternalNotes`)

---

## 3. Column & Data Types

### Column and Data Type Standards

Proper column naming and data type selection ensures consistency and optimal performance.

### Column Naming

- **Casing:** Use UpperCamelCase for all column names (e.g., `FirstName`)
- **Primary Keys:** All primary keys should be named simply `ID`
- **Foreign Keys:** Name should reference the source table (e.g., `ApplicantID`). If context is needed, prefix the name (e.g., `CreatedByUserID`)
- **Clarity:** Avoid abbreviations. Use `DateOfBirth` instead of `DOB`

### Standard Data Types

- **Text:** `NVARCHAR(n)` for all text data to support Unicode characters. Avoid `VARCHAR`
- **Date & Time:** `DATETIME2(7)` for all date and time storage. Avoid `DATETIME`
- **Whole Numbers:** `INT` or `BIGINT`
- **Decimal Numbers:** `DECIMAL(p, s)` for financial calculations

---

## 4. Enum Tables

### "Enum" Tables for Static Values

To manage static sets of values (like statuses or types), dedicated tables must be created.

### Key Principles

- **Immutability (Recommended):** The primary key (`ID`) values in these tables must be treated as permanent and should not be changed.
- **Mutable ID Exception:** If an ID might change, the table must include an additional, immutable text-based `Key` column.

### Enum Functions (Avoiding Magic Numbers)

Create functions following the pattern `Enums_[TableName]_[Value]`

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

### Performance & When to Use Which

- **View:** Best for non-parameterized, reusable query logic. Performance is excellent
- **Inline TVF (iTVF):** Best for parameterized, reusable query logic. Just as performant as a view
- **Multi-statement TVF (mTVF):** AVOID using in joins. This is a known performance bottleneck

---

## 6. Stored Procedures

### Stored Procedure & Object Naming

All stored procedures and functions must follow the `Entity_Action` pattern, where Entity is the main table/object being operated on.

### Common Stored Procedure Examples

```sql
-- Single entity operations
Applicants_GetByID
Applicants_GetByGuid
Applicants_Insert
Applicants_Update
Applicants_Delete
Applicants_GetActive
Applicants_GetPending

-- Multiple entity operations (use primary entity)
ApplicantSessions_GetWithApplicantDetails
Orders_GetWithCustomerInfo

-- Batch operations
Applicants_ProcessPending
Orders_CalculateTotals
Verifications_ExpireOld
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

**Important:** Never use prefixes like `sp_` (reserved for system procedures), `proc_`, or `usp_`. The `Entity_Action` pattern is sufficient.

---

## 7. T-SQL Formatting

### T-SQL Formatting and Style

Consistent formatting improves readability and maintainability of SQL code.

### Formatting Rules

- **Keywords:** All T-SQL keywords must be in UPPERCASE (`SELECT`, `FROM`, `WHERE`)
- **Indentation:** Use consistent indentation (4 spaces recommended)
- **Line Breaks:** Place each major clause on a new line
- **Column Lists:** One column per line in SELECT statements for better readability
- **Alignment:** Align column names and values for clarity

### Example

```sql
SELECT
    A.ID,
    A.FirstName,
    A.LastName,
    A.Email,
    A.DateOfBirth
FROM dbo.TB_Applicants A WITH(NOLOCK)
INNER JOIN dbo.TB_ApplicantSessions S WITH(NOLOCK) ON S.ApplicantID = A.ID
WHERE
    A.IsActive = 1
    AND S.StatusID = dbo.Enums_Status_Active()
ORDER BY
    A.LastName,
    A.FirstName
```

---

## 8. Commenting

### Code Comments

Use comments to explain complex logic, business rules, or non-obvious implementation details.

```sql
-- Calculate applicant age based on current date
-- Returns NULL if DateOfBirth is in the future
CREATE FUNCTION dbo.Applicants_GetAge(@DateOfBirth DATE)
RETURNS INT
AS
BEGIN
    -- Validate date is not in future
    IF @DateOfBirth > CAST(GETDATE() AS DATE)
        RETURN NULL
    
    -- Calculate years difference
    RETURN DATEDIFF(YEAR, @DateOfBirth, GETDATE())
        - CASE
            WHEN MONTH(@DateOfBirth) > MONTH(GETDATE()) THEN 1
            WHEN MONTH(@DateOfBirth) = MONTH(GETDATE())
                AND DAY(@DateOfBirth) > DAY(GETDATE()) THEN 1
            ELSE 0
          END
END
GO
```

---

## 9. Indexing Strategy

### Index Design Principles

- Create indexes on foreign keys used in JOIN conditions
- Index columns frequently used in WHERE clauses
- Consider covering indexes for frequently run queries
- Monitor index usage and remove unused indexes
- Use included columns for covering indexes instead of adding to key

### Example

```sql
-- Foreign key index
CREATE NONCLUSTERED INDEX IX_ApplicantSessions_ApplicantID
ON dbo.TB_ApplicantSessions(ApplicantID)

-- Covering index with included columns
CREATE NONCLUSTERED INDEX IX_Applicants_Email_Covering
ON dbo.TB_Applicants(Email)
INCLUDE (FirstName, LastName, DateOfBirth)
```

---

## 10. Transactions

### Transaction Usage

Use explicit transactions for operations that must succeed or fail as a unit.

```sql
BEGIN TRANSACTION

BEGIN TRY
    -- Insert applicant
    INSERT INTO dbo.TB_Applicants (FirstName, LastName, Email)
    VALUES (@FirstName, @LastName, @Email)
    
    DECLARE @ApplicantID INT = SCOPE_IDENTITY()
    
    -- Insert related session
    INSERT INTO dbo.TB_ApplicantSessions (ApplicantID, StatusID)
    VALUES (@ApplicantID, dbo.Enums_Status_Pending())
    
    COMMIT TRANSACTION
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    THROW
END CATCH
```

---

## 11. Dynamic SQL

### Dynamic SQL Guidelines

Use dynamic SQL sparingly and always with parameterization to prevent SQL injection.

```sql
DECLARE @SQL NVARCHAR(MAX)
DECLARE @TableName NVARCHAR(128) = 'TB_Applicants'

-- Use sp_executesql with parameters
SET @SQL = N'SELECT * FROM ' + QUOTENAME(@TableName) + N' WHERE ID = @ID'

EXEC sp_executesql @SQL,
    N'@ID INT',
    @ID = 123
```

---

## 12. WITH (NOLOCK)

### NOLOCK Usage

Use `WITH(NOLOCK)` hint for read operations where dirty reads are acceptable and performance is critical.

**Caution:** NOLOCK can return uncommitted data and may skip or duplicate rows.

```sql
-- Acceptable for reporting queries
SELECT *
FROM dbo.TB_Applicants WITH(NOLOCK)
WHERE StatusID = dbo.Enums_Status_Active()

-- Never use for financial or critical data
SELECT Balance
FROM dbo.TB_Accounts -- No NOLOCK here
WHERE AccountID = @AccountID
```

---

## 13. Cursors

### Cursor Guidelines

**Avoid cursors whenever possible.** Use set-based operations instead.

If a cursor is absolutely necessary:
- Use `FAST_FORWARD` cursor type
- Keep cursor scope as small as possible
- Deallocate cursor immediately after use

```sql
-- Set-based approach (preferred)
UPDATE dbo.TB_Applicants
SET LastProcessedDate = GETDATE()
WHERE StatusID = dbo.Enums_Status_Pending()

-- Cursor approach (avoid if possible)
DECLARE @ApplicantID INT
DECLARE cursor_applicants CURSOR FAST_FORWARD FOR
    SELECT ID FROM dbo.TB_Applicants WHERE StatusID = dbo.Enums_Status_Pending()

OPEN cursor_applicants
FETCH NEXT FROM cursor_applicants INTO @ApplicantID

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Process each applicant
    EXEC dbo.Applicants_Process @ApplicantID
    
    FETCH NEXT FROM cursor_applicants INTO @ApplicantID
END

CLOSE cursor_applicants
DEALLOCATE cursor_applicants
```

---

## 14. SARGable Queries

### Writing SARGable Queries

SARGable (Search ARGument ABLE) queries allow the query optimizer to use indexes effectively.

**Bad (Non-SARGable):**
```sql
-- Function on column prevents index usage
WHERE YEAR(DateOfBirth) = 1990

-- Implicit conversion prevents index usage
WHERE PhoneNumber = 1234567890 -- PhoneNumber is NVARCHAR
```

**Good (SARGable):**
```sql
-- Use date range instead
WHERE DateOfBirth >= '1990-01-01' AND DateOfBirth < '1991-01-01'

-- Explicit conversion or correct type
WHERE PhoneNumber = N'1234567890'
```

---

## 15. SELECT *

### Avoid SELECT *

Always specify column names explicitly instead of using `SELECT *`.

**Benefits:**
- Better performance (only retrieves needed columns)
- Protects against schema changes
- Self-documenting code
- Enables covering indexes

```sql
-- Bad
SELECT * FROM dbo.TB_Applicants

-- Good
SELECT
    ID,
    FirstName,
    LastName,
    Email,
    DateOfBirth
FROM dbo.TB_Applicants
```

---

## 16. Temporary Objects

### Temporary Tables vs Table Variables

**Use Temporary Tables (#temp) when:**
- Working with large datasets (>100 rows)
- Need statistics for query optimization
- Require indexes

**Use Table Variables (@table) when:**
- Small datasets (<100 rows)
- Simple operations
- Want to avoid recompilation

```sql
-- Temporary table
CREATE TABLE #TempApplicants
(
    ID INT,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100)
)

INSERT INTO #TempApplicants
SELECT ID, FirstName, LastName
FROM dbo.TB_Applicants
WHERE StatusID = dbo.Enums_Status_Active()

-- Use and clean up
SELECT * FROM #TempApplicants
DROP TABLE #TempApplicants

-- Table variable
DECLARE @ApplicantIDs TABLE
(
    ID INT
)

INSERT INTO @ApplicantIDs
SELECT ID FROM dbo.TB_Applicants WHERE IsActive = 1
```

---

## 17. Schema Binding

### Using SCHEMABINDING

Use `WITH SCHEMABINDING` for views and functions to prevent underlying table changes that would break them.

```sql
CREATE VIEW dbo.VW_ActiveApplicants
WITH SCHEMABINDING
AS
    SELECT
        A.ID,
        A.FirstName,
        A.LastName,
        A.Email
    FROM dbo.TB_Applicants A
    WHERE A.IsActive = 1
GO
```

---

# C# Coding Standards

## 1. Naming Conventions

### General Naming Rules

- **PascalCase:** Classes, methods, properties, public fields
- **camelCase:** Local variables, method parameters, private fields
- **_camelCase:** Private fields (with underscore prefix)
- **UPPER_CASE:** Constants

```csharp
public class ApplicantService
{
    private readonly IApplicantRepository _repository;
    private const int MAX_RETRY_COUNT = 3;
    
    public async Task<Applicant> GetApplicantAsync(int applicantId)
    {
        var applicant = await _repository.GetByIdAsync(applicantId);
        return applicant;
    }
}
```

### Meaningful Names

```csharp
// Bad
var d = DateTime.Now;
var list = GetData();

// Good
var currentDate = DateTime.Now;
var activeApplicants = GetActiveApplicants();
```

---

## 2. Architecture

### Layered Architecture

Organize code into logical layers:

- **API Layer:** Controllers, request/response models
- **Business Layer:** Services, business logic
- **Data Layer:** Repositories, database access
- **Domain Layer:** Entities, value objects

### Dependency Injection

```csharp
public class ApplicantService : IApplicantService
{
    private readonly IApplicantRepository _repository;
    private readonly ILogger<ApplicantService> _logger;
    private readonly IEmailService _emailService;
    
    public ApplicantService(
        IApplicantRepository repository,
        ILogger<ApplicantService> logger,
        IEmailService emailService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }
}
```

---

## 3. Formatting

### Code Formatting Guidelines

- **Braces:** Always use braces, even for single-line statements
- **Indentation:** 4 spaces (not tabs)
- **Line Length:** Maximum 120 characters
- **Blank Lines:** Use to separate logical blocks

```csharp
// Bad
if (isValid)
    return true;

// Good
if (isValid)
{
    return true;
}

// Good - logical separation
public async Task<Result> ProcessApplicantAsync(int applicantId)
{
    var applicant = await _repository.GetByIdAsync(applicantId);
    
    if (applicant == null)
    {
        return Result.NotFound();
    }
    
    var validationResult = ValidateApplicant(applicant);
    
    if (!validationResult.IsValid)
    {
        return Result.ValidationError(validationResult.Errors);
    }
    
    await _emailService.SendWelcomeEmailAsync(applicant.Email);
    
    return Result.Success();
}
```

---

## 4. DTOs

### Data Transfer Objects

Use DTOs for API requests and responses. Keep them separate from domain entities.

```csharp
// Request DTO
public class CreateApplicantRequest
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    public DateTime DateOfBirth { get; set; }
}

// Response DTO
public class ApplicantResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Status { get; set; }
}
```

---

## 5. Async/Await

### Asynchronous Programming

Always use async/await for I/O operations (database, HTTP, file system).

```csharp
// Correct async usage
public async Task<Applicant> GetApplicantAsync(int id)
{
    var applicant = await _repository.GetByIdAsync(id);
    
    if (applicant != null)
    {
        await _auditService.LogAccessAsync(id, "GetApplicant");
    }
    
    return applicant;
}

// Don't block on async methods
// Bad
var result = SomeAsyncMethod().Result;

// Good
var result = await SomeAsyncMethod();
```

### Async Method Naming

```csharp
// All async methods should end with "Async"
public async Task<List<Applicant>> GetActiveApplicantsAsync()
{
    return await _repository.GetActiveAsync();
}
```

---

## 6. Error Handling

### Exception Handling

```csharp
public async Task<Result<Applicant>> CreateApplicantAsync(CreateApplicantRequest request)
{
    try
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<Applicant>.ValidationError("Email is required");
        }
        
        // Check for duplicates
        var existing = await _repository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            return Result<Applicant>.Conflict("Applicant with this email already exists");
        }
        
        // Create applicant
        var applicant = new Applicant
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };
        
        await _repository.InsertAsync(applicant);
        
        return Result<Applicant>.Success(applicant);
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error creating applicant");
        return Result<Applicant>.Error("An error occurred while creating the applicant");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating applicant");
        throw;
    }
}
```

---

## 7. Configuration

### Configuration Management

Use `IConfiguration` and strongly-typed options.

```csharp
// appsettings.json
{
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "FromAddress": "noreply@example.com"
  }
}

// Options class
public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; }
}

// Startup configuration
services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));

// Usage in service
public class EmailService
{
    private readonly EmailSettings _settings;
    
    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Use _settings.SmtpServer, etc.
    }
}
```

---

## 8. Best Practices

### General Best Practices

- **SOLID Principles:** Follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **DRY:** Don't Repeat Yourself - extract common logic
- **KISS:** Keep It Simple, Stupid - avoid over-engineering
- **YAGNI:** You Aren't Gonna Need It - don't add functionality until needed

```csharp
// Bad - violates Single Responsibility
public class ApplicantService
{
    public void CreateApplicant() { }
    public void SendEmail() { }
    public void LogToDatabase() { }
    public void GenerateReport() { }
}

// Good - each class has single responsibility
public class ApplicantService
{
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IReportGenerator _reportGenerator;
    
    public async Task CreateApplicantAsync(CreateApplicantRequest request)
    {
        // Only handles applicant creation
        // Delegates other responsibilities
    }
}
```

---

## 9. Documentation

### Code Documentation

Use XML comments for public APIs.

```csharp
/// <summary>
/// Retrieves an applicant by their unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the applicant.</param>
/// <returns>The applicant if found; otherwise, null.</returns>
/// <exception cref="ArgumentException">Thrown when id is less than or equal to zero.</exception>
public async Task<Applicant> GetApplicantAsync(int id)
{
    if (id <= 0)
    {
        throw new ArgumentException("Applicant ID must be greater than zero", nameof(id));
    }
    
    return await _repository.GetByIdAsync(id);
}
```

---

## 10. DI Scope Rules

### Dependency Injection Scopes

- **Transient:** Created each time requested. Use for lightweight, stateless services
- **Scoped:** Created once per request. Use for request-specific services (e.g., DbContext, repositories)
- **Singleton:** Created once for application lifetime. Use for stateless services

```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Singleton - one instance for entire application
    services.AddSingleton<IEmailService, EmailService>();
    
    // Scoped - one instance per HTTP request
    services.AddScoped<IApplicantRepository, ApplicantRepository>();
    services.AddScoped<SqlConnection>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new SqlConnection(config.GetConnectionString("Default"));
    });
    
    // Transient - new instance every time
    services.AddTransient<IApplicantValidator, ApplicantValidator>();
}
```

---

## 11. Logging Standards

### Structured Logging

Use structured logging with placeholders, not string concatenation.

```csharp
public class ApplicantService
{
    private readonly ILogger<ApplicantService> _logger;
    
    public async Task<Applicant> GetApplicantAsync(int id)
    {
        _logger.LogInformation("Retrieving applicant {ApplicantId}", id);
        
        try
        {
            var applicant = await _repository.GetByIdAsync(id);
            
            if (applicant == null)
            {
                _logger.LogWarning("Applicant {ApplicantId} not found", id);
                return null;
            }
            
            _logger.LogInformation(
                "Successfully retrieved applicant {ApplicantId} - {FirstName} {LastName}",
                id,
                applicant.FirstName,
                applicant.LastName
            );
            
            return applicant;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving applicant {ApplicantId}",
                id
            );
            throw;
        }
    }
}
```

### Log Levels

- **Trace:** Very detailed, typically only for debugging
- **Debug:** Detailed information for diagnosing problems
- **Information:** General informational messages
- **Warning:** Indicates potential problems
- **Error:** Error events that might still allow the application to continue
- **Critical:** Critical failures requiring immediate attention

---

## 12. Background Jobs

### Hangfire Background Jobs

```csharp
// Startup registration
services.AddHangfire(config =>
{
    config.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire"));
});

// Enqueue a job
BackgroundJob.Enqueue<IApplicantService>(service => 
    service.ProcessPendingApplicantsAsync()
);

// Schedule a delayed job
BackgroundJob.Schedule<IApplicantService>(
    service => service.SendReminderEmailAsync(applicantId),
    TimeSpan.FromHours(24)
);

// Recurring job
RecurringJob.AddOrUpdate<IApplicantService>(
    "process-expired-applications",
    service => service.ProcessExpiredApplicationsAsync(),
    Cron.Daily
);
```

---

## 13. EMQX/MQTT

### MQTT Client Usage

```csharp
public class MqttService : IMqttService
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttService> _logger;
    
    public async Task PublishAsync(string topic, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            
            await _mqttClient.PublishAsync(message);
            
            _logger.LogInformation(
                "Published message to topic {Topic}",
                topic
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing message to topic {Topic}",
                topic
            );
            throw;
        }
    }
}
```

---

## 14. Authorization

### Authorization Patterns

```csharp
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetUsers()
    {
        // Only accessible by users with "CanViewUsers" policy
    }
}

// Policy-based authorization setup
services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewUsers", policy =>
        policy.RequireClaim("Permission", "Users.View"));
    
    options.AddPolicy("CanEditUsers", policy =>
        policy.RequireClaim("Permission", "Users.Edit"));
});
```

---

## 15. Configuration

### Environment-Specific Configuration

```json
// appsettings.json (base configuration)
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=MyDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}

// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}

// appsettings.Production.json
{
  "ConnectionStrings": {
    "Default": "Server=prod-server;Database=MyDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## 16. Request Validation

### Input Validation

```csharp
public class CreateApplicantRequest
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(DateValidation), nameof(DateValidation.ValidateDateOfBirth))]
    public DateTime DateOfBirth { get; set; }
}

public static class DateValidation
{
    public static ValidationResult ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth > DateTime.Today)
        {
            return new ValidationResult("Date of birth cannot be in the future");
        }
        
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (age < 18)
        {
            return new ValidationResult("Applicant must be at least 18 years old");
        }
        
        return ValidationResult.Success;
    }
}
```

---

## 17. Error Responses (RFC 7807)

### Problem Details (RFC 7807)

```csharp
public class ErrorResponse
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }
    
    /// <summary>
    /// A human-readable explanation specific to this occurrence
    /// </summary>
    public string Detail { get; set; }
    
    /// <summary>
    /// A URI reference that identifies the specific occurrence
    /// </summary>
    public string Instance { get; set; }
    
    /// <summary>
    /// Request trace identifier for tracking
    /// </summary>
    public string TraceId { get; set; }
    
    /// <summary>
    /// Additional error-specific details
    /// </summary>
    public Dictionary<string, object> Extensions { get; set; }
}

// Example usage
public IActionResult ValidationError(string message)
{
    var error = new ErrorResponse
    {
        Type = "validation_error",
        Title = "Validation Error",
        Status = 400,
        Detail = message,
        Instance = HttpContext.Request.Path,
        TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    };
    
    return BadRequest(error);
}
```

---

## 18. Custom Exceptions

### Custom Exception Classes

```csharp
public class ValidationException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(string errorCode)
        : base($"Validation failed: {errorCode}")
    {
        ErrorCode = errorCode;
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(string errorCode, Dictionary<string, string[]> errors)
        : base($"Validation failed: {errorCode}")
    {
        ErrorCode = errorCode;
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public string ResourceType { get; }
    public object ResourceId { get; }
    
    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID {resourceId} not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

public class BusinessRuleException : Exception
{
    public string RuleCode { get; }
    
    public BusinessRuleException(string ruleCode, string message)
        : base(message)
    {
        RuleCode = ruleCode;
    }
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
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (BusinessRuleException ex)
        {
            await HandleBusinessRuleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        _logger.LogWarning(
            exception,
            "Validation error: {ErrorCode}",
            exception.ErrorCode
        );
        
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            Type = "validation_error",
            Title = "Validation Error",
            Status = 400,
            Detail = exception.Message,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
            Extensions = new Dictionary<string, object>
            {
                ["errorCode"] = exception.ErrorCode,
                ["errors"] = exception.Errors
            }
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
    
    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred"
        );
        
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var response = new ErrorResponse
        {
            Type = "internal_error",
            Title = "Internal Server Error",
            Status = 500,
            Detail = "An unexpected error occurred. Please try again later.",
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

---

## 19. Error Handling Best Practices

### Error Handling Guidelines

1. **Never swallow exceptions** - always log or rethrow
2. **Use specific exception types** - helps with handling and debugging
3. **Include context in exceptions** - add relevant details
4. **Log before throwing** - capture state before exception propagates
5. **Use try-catch at boundaries** - API controllers, background jobs
6. **Return proper HTTP status codes** - 400 for validation, 404 for not found, 500 for server errors

```csharp
public async Task<Result<Applicant>> ProcessApplicantAsync(int applicantId)
{
    _logger.LogInformation(
        "Starting applicant processing for {ApplicantId}",
        applicantId
    );
    
    try
    {
        var applicant = await _repository.GetByIdAsync(applicantId);
        
        if (applicant == null)
        {
            _logger.LogWarning(
                "Applicant {ApplicantId} not found",
                applicantId
            );
            throw new NotFoundException("Applicant", applicantId);
        }
        
        // Validate business rules
        if (applicant.Status != ApplicantStatus.Pending)
        {
            throw new BusinessRuleException(
                "INVALID_STATUS",
                "Only pending applicants can be processed"
            );
        }
        
        // Process applicant
        await ProcessApplicantLogicAsync(applicant);
        
        _logger.LogInformation(
            "Successfully processed applicant {ApplicantId}",
            applicantId
        );
        
        return Result<Applicant>.Success(applicant);
    }
    catch (NotFoundException)
    {
        throw; // Re-throw known exceptions
    }
    catch (BusinessRuleException)
    {
        throw; // Re-throw known exceptions
    }
    catch (SqlException ex)
    {
        _logger.LogError(
            ex,
            "Database error processing applicant {ApplicantId}",
            applicantId
        );
        throw new DataAccessException(
            "Error accessing database",
            ex
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Unexpected error processing applicant {ApplicantId}",
            applicantId
        );
        throw; // Re-throw unexpected exceptions
    }
}
```

---

## 20. Observability

### Logging, Metrics, and Tracing

#### Graylog Configuration

Configure Graylog via environment variables in Kubernetes ConfigMap:

| Variable | Default | Description |
|----------|---------|-------------|
| `UPI_ENVIRONMENT` | `ASPNETCORE_ENVIRONMENT` | Environment name (dev, preprod, prod) |
| `UPI_APP_NAME` | Assembly name | Application name for logs |
| `UPI_LOG_LEVEL` | Information | Default log level |
| `UPI_LOG_LEVEL_MICROSOFT` | Warning | Microsoft.* namespace level |
| `UPI_LOG_LEVEL_SYSTEM` | Warning | System.* namespace level |
| `UPI_GRAYLOG_HOST` | Required | Graylog server address |
| `UPI_GRAYLOG_PORT` | 12201 | GELF port |
| `UPI_GRAYLOG_TRANSPORT` | Udp | Udp / Tcp / Http |
| `UPI_GRAYLOG_FACILITY` | → UPI_APP_NAME | Facility name |
| `UPI_GRAYLOG_BUFFER_SIZE` | 1000 | Async buffer size |
| `UPI_INCLUDE_DEBUG_INFO` | false | Include stack trace (dev/preprod only) |

#### Searching Logs in Graylog

```
# Find all errors for an application
Application:"Verification.API" AND Level:"Error"

# Find by TraceId (from error response)
TraceId:"0HN4DVGL9SK2T"

# Find by environment
Environment:"production" AND Application:"OneAdmin.API"

# Find validation errors
Message:"validation_error"

# Find errors in last hour
Application:"Verification.API" AND Level:"Error" AND timestamp:[now-1h TO now]
```

#### Customer Support Flow

1. Customer reports: "I got an error when creating an applicant"
2. Support asks: "What's the error traceId?"
   - Customer: "0HN4DVGL9SK2T"
3. Support searches Graylog: `TraceId:"0HN4DVGL9SK2T"`
4. Graylog shows complete error context with request details
5. Support provides specific solution based on error details

#### Request Body Sanitization

Request bodies are automatically sanitized before logging:

- **Data URIs (images):** `"data:image/png;base64,..."` → `"[BASE64_IMAGE:2.3MB]"`
- **Raw base64 strings > 100 chars** → `"[BASE64_DATA:156KB]"`
- **Long binary-like strings** → `"[BINARY_DATA:50KB]"`
- **Long strings > 500 chars** → `"First 500 chars...[TRUNCATED:12KB]"`
- **Sensitive fields** (password, token, secret, apikey) → `"[REDACTED]"`
- **Entire body > 10KB** → `"[BODY_TRUNCATED:15KB]"`

**Important:** Never log sensitive data (passwords, tokens, personal data). Use structured logging with placeholders.

---

## Related Documentation

- Upgaming Platform Overview
- Getting Started Guide

---

*Last Updated: January 2025*
