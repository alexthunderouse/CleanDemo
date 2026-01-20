namespace CleanAPIDemo.Application.Features.Products.v1;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price);
