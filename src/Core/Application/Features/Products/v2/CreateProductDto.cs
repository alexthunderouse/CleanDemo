namespace CleanAPIDemo.Application.Features.Products.v2;

/// <summary>
/// V2 Create Product DTO with optional category field.
/// </summary>
public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    string? Category);
