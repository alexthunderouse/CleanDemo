namespace CleanAPIDemo.Application.Products.DTOs.V2;

/// <summary>
/// V2 Create Product DTO with optional category field.
/// </summary>
public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    string? Category);
