namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record ProductSummaryViewDto(
    Guid Id,
    string Name,
    decimal Price,
    string PriceCategory,
    int DaysSinceCreated);
