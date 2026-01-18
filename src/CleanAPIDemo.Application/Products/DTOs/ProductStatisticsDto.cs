namespace CleanAPIDemo.Application.Products.DTOs;

public record ProductStatisticsDto(
    int TotalProducts,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    string PriceCategory);
