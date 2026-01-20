namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price);
