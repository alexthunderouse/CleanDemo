using CleanAPIDemo.Application.Features.Products.v1;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
