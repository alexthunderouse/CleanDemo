using CleanAPIDemo.Application.Products.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductsByCategory;

public record GetProductsByCategoryQuery(string PriceCategory) : IRequest<IEnumerable<ProductByCategoryDto>>;
