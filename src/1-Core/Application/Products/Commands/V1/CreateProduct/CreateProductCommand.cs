using CleanAPIDemo.Application.Products.DTOs.V1;
using MediatR;

namespace CleanAPIDemo.Application.Products.Commands.V1.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price) : IRequest<ProductDto>;
