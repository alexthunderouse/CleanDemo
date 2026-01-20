using CleanAPIDemo.Application.Features.Products.v2;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v2.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string? Category) : IRequest<ProductDto>;
