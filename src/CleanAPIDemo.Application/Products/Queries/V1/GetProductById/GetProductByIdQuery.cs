using CleanAPIDemo.Application.Products.DTOs.V1;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.V1.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
