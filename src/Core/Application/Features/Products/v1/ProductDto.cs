namespace CleanAPIDemo.Application.Features.Products.v1;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    DateTime CreatedAt);
