using CleanAPIDemo.Application.Products.DTOs.V1;
using MediatR;

namespace CleanAPIDemo.Application.Products.Queries.V1.GetProductSummaryView;

public record GetProductSummaryViewQuery : IRequest<IEnumerable<ProductSummaryViewDto>>;
