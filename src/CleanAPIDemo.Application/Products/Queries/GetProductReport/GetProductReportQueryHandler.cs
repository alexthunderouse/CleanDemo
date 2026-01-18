using CleanAPIDemo.Application.Common.Mappings;
using CleanAPIDemo.Application.Products.DTOs;
using CleanAPIDemo.Domain.Interfaces;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductReport;

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
