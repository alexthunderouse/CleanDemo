using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Application.Features.Products.v1;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductsByCategory;

public class GetProductsByCategoryQueryHandler
    : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductByCategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductMapper _mapper;

    public GetProductsByCategoryQueryHandler(IUnitOfWork unitOfWork, ProductMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductByCategoryDto>> Handle(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _unitOfWork.ProductProcedures
            .GetProductsByCategoryAsync(request.PriceCategory, cancellationToken);

        return results.Select(_mapper.ToByCategoryDto);
    }
}
