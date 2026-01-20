namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record ProductByCategoryDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCategory);
