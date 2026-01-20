using CleanAPIDemo.Application.Products.DTOs.V1;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.V1.GetProductsByCategory;

public record GetProductsByCategoryQuery(string PriceCategory) : IRequest<IEnumerable<ProductByCategoryDto>>;
