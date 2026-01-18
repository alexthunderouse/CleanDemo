namespace CleanAPIDemo.Application.Products.DTOs;

public record ProductByCategoryDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCategory);
