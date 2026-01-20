using CleanAPIDemo.Application.Features.Products.v1;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(string PriceCategory) : IRequest<IEnumerable<ProductByCategoryDto>>;
