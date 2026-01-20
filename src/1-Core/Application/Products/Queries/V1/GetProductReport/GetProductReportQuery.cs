using CleanAPIDemo.Application.Products.DTOs.V1;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.V1.GetProductReport;

public record GetProductReportQuery(decimal? MinPrice = null) : IRequest<ProductReportDto>;
