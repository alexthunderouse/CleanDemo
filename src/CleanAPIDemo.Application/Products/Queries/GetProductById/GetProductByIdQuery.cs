using CleanAPIDemo.Application.Products.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
