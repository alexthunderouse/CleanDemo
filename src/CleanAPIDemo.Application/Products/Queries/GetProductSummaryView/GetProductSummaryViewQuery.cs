using CleanAPIDemo.Application.Products.DTOs;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.GetProductSummaryView;

public record GetProductSummaryViewQuery : IRequest<IEnumerable<ProductSummaryViewDto>>;
