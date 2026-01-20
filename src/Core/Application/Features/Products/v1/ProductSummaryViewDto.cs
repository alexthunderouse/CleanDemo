namespace CleanAPIDemo.Application.Features.Products.v1;

public record ProductSummaryViewDto(
    Guid Id,
    string Name,
    decimal Price,
    string PriceCategory,
    int DaysSinceCreated);
