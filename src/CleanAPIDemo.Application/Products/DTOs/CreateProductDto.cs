namespace CleanAPIDemo.Application.Products.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price);
