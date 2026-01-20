namespace CleanAPIDemo.Application.Features.Products.v1;

public record ProductByCategoryDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string PriceCategory);
