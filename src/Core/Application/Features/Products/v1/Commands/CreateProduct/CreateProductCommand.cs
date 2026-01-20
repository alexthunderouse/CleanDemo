using CleanAPIDemo.Application.Features.Products.v1;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<ProductDto>;
