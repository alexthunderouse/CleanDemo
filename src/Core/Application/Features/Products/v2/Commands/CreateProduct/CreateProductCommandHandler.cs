using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Application.Features.Products.v2;
using CleanAPIDemo.Domain.Entities;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v2.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductMapper _mapper;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, ProductMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.ToProductDtoV2(product);
    }
}
