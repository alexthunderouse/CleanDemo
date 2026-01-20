namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    DateTime CreatedAt);
