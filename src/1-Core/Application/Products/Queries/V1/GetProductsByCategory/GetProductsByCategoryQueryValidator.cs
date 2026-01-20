using FluentValidation;

namespace CleanAPIDemo.Application.Products.Queries.V1.GetProductsByCategory;

public class GetProductsByCategoryQueryValidator : AbstractValidator<GetProductsByCategoryQuery>
{
    private static readonly string[] ValidCategories = ["Budget", "Mid-Range", "Premium", "Luxury"];

    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.PriceCategory)
            .NotEmpty().WithMessage("Price category is required.")
            .Must(category => ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Price category must be one of: {string.Join(", ", ValidCategories)}.");
    }
}
