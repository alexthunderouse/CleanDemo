using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Application.Features.Products.v1;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductSummaryView;

public class GetProductSummaryViewQueryHandler
    : IRequestHandler<GetProductSummaryViewQuery, IEnumerable<ProductSummaryViewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductMapper _mapper;

    public GetProductSummaryViewQueryHandler(IUnitOfWork unitOfWork, ProductMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductSummaryViewDto>> Handle(
        GetProductSummaryViewQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _unitOfWork.ProductProcedures
            .GetProductSummaryViewAsync(cancellationToken);

        return results.Select(_mapper.ToSummaryViewDto);
    }
}
