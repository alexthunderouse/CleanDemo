using CleanAPIDemo.Application.Features.Products.v1;
using MediatR;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductSummaryView;

public record GetProductSummaryViewQuery : IRequest<IEnumerable<ProductSummaryViewDto>>;
