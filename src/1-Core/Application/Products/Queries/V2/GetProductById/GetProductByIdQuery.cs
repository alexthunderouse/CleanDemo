using CleanAPIDemo.Application.Products.DTOs.V2;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.V2.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
