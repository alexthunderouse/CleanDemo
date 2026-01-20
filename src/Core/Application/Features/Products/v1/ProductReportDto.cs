namespace CleanAPIDemo.Application.Features.Products.v1;

public record ProductReportDto(
    IEnumerable<ProductByCategoryDto> Products,
    IEnumerable<ProductStatisticsDto> Statistics);
