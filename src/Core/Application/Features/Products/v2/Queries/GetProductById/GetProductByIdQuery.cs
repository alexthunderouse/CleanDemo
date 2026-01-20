using CleanAPIDemo.Application.Features.Products.v2;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v2.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
