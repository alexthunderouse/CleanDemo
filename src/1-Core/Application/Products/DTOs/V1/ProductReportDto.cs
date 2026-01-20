namespace CleanAPIDemo.Application.Products.DTOs.V1;

public record ProductReportDto(
    IEnumerable<ProductByCategoryDto> Products,
    IEnumerable<ProductStatisticsDto> Statistics);
