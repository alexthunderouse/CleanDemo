using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Application.Features.Products.v1;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductMapper _mapper;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, ProductMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);

        return product is null ? null : _mapper.ToProductDto(product);
    }
}
