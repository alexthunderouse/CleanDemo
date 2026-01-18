using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.DTOs;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductSummaryView;

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
