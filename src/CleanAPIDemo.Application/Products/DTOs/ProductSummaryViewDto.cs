namespace CleanAPIDemo.Application.Products.DTOs;

public record ProductSummaryViewDto(
    Guid Id,
    string Name,
    decimal Price,
    string PriceCategory,
    int DaysSinceCreated);
