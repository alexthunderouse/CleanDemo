using FluentValidation;

namespace CleanAPIDemo.Application.Features.Products.v1.Queries.GetProductsByCategory;

public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    private static readonly string[] ValidCategories = ["Budget", "Mid-Range", "Premium", "Luxury"];

    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.PriceCategory)
            .NotEmpty().WithMessage("price_category_required")
            .Must(category => ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            .WithMessage("price_category_invalid");
    }
}
