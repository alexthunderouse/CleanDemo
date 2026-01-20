using CleanAPIDemo.Application.Products.DTOs.V2;
using MediatR;

namespace CleanAPIDemo.Application.Products.Commands.V2.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string? Category) : IRequest<ProductDto>;
