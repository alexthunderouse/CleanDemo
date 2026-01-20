namespace CleanAPIDemo.Application.Features.Products.v2;

/// <summary>
/// V2 Product DTO with enhanced structure including audit timestamps.
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    ProductAuditInfo Audit);

/// <summary>
/// Audit information for product tracking.
/// </summary>
public record ProductAuditInfo(
    DateTime CreatedAt,
    DateTime? UpdatedAt);
