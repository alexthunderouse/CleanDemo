using FluentValidation;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductReport;

public class GetProductReportQueryValidator : AbstractValidator<GetProductReportQuery>
{
    public GetProductReportQueryValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("Minimum price must be greater than or equal to zero.");
    }
}
