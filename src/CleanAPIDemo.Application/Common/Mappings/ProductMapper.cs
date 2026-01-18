using CleanAPIDemo.Application.Products.DTOs;
using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Entities.Views;
using Riok.Mapperly.Abstractions;

namespace CleanAPIDemo.Application.Common.Mappings;

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto ToDto(Product product);
    public partial Product ToEntity(CreateProductDto dto);
    public partial ProductByCategoryDto ToByCategoryDto(ProductByCategoryResult result);
    public partial ProductStatisticsDto ToStatisticsDto(ProductStatisticsResult result);
    public partial ProductSummaryViewDto ToSummaryViewDto(ProductSummaryView view);
}
