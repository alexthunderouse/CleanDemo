using CleanAPIDemo.Application.Mappings;
using CleanAPIDemo.Application.Features.Products.v1;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductReport;

public class GetProductReportQueryHandler : IRequestHandler<GetProductReportQuery, ProductReportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductMapper _mapper;

    public GetProductReportQueryHandler(IUnitOfWork unitOfWork, ProductMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductReportDto> Handle(
        GetProductReportQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.ProductProcedures
            .GetProductReportAsync(request.MinPrice, cancellationToken);

        return new ProductReportDto(
            Products: result.Products.Select(_mapper.ToByCategoryDto),
            Statistics: result.Statistics.Select(_mapper.ToStatisticsDto));
    }
}
