namespace CleanAPIDemo.Application.Products.DTOs;

public record ProductReportDto(
    IEnumerable<ProductByCategoryDto> Products,
    IEnumerable<ProductStatisticsDto> Statistics);
