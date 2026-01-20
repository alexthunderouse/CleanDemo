namespace CleanAPIDemo.Application.Features.Products.v1;

public record ProductStatisticsDto(
    int TotalProducts,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    string PriceCategory);
