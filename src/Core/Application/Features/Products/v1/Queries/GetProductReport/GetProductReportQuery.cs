using CleanAPIDemo.Application.Features.Products.v1;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductReport;

public record GetProductReportQuery(decimal? MinPrice = null) : IRequest<ProductReportDto>;
