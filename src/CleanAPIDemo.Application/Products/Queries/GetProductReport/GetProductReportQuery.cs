using CleanAPIDemo.Application.Products.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductReport;

public record GetProductReportQuery(decimal? MinPrice = null) : IRequest<ProductReportDto>;
