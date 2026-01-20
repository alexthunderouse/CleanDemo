namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record ProductStatisticsDto(
    int TotalProducts,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    string PriceCategory);
