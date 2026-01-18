using CleanAPIDemo.Application.Products.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<ProductDto>;
