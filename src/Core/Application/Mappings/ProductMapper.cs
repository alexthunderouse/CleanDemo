using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Entities.StoredProcedures;
using CleanAPIDemo.Domain.Entities.Views;
using Riok.Mapperly.Abstractions;
using V1 = CleanAPIDemo.Application.Features.Products.v1;
using V2 = CleanAPIDemo.Application.Features.Products.v2;

namespace CleanAPIDemo.Application.Mappings;

[Mapper]
public partial class ProductMapper
{
    // V1 Mappings
    public V1.ProductDto ToProductDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CreatedAt);

    public Product ToEntity(V1.CreateProductDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price
    };

    public V1.ProductByCategoryDto ToByCategoryDto(ProductByCategoryResult result) => new(
        result.Id,
        result.Name,
        result.Description,
        result.Price,
        result.PriceCategory);

    public V1.ProductStatisticsDto ToStatisticsDto(ProductStatisticsResult result) => new(
        result.TotalProducts,
        result.AveragePrice,
        result.MinPrice,
        result.MaxPrice,
        result.PriceCategory);

    public V1.ProductSummaryViewDto ToSummaryViewDto(ProductSummaryView view) => new(
        view.Id,
        view.Name,
        view.Price,
        view.PriceCategory,
        view.DaysSinceCreated);

    // V2 Mappings
    public V2.ProductDto ToProductDtoV2(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        new V2.ProductAuditInfo(product.CreatedAt, product.UpdatedAt));
}
