using FluentValidation;

namespace CleanAPIDemo.Application.Features.Products.v1.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("name_required")
            .MaximumLength(200).WithMessage("name_max_length");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("description_max_length");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("price_must_be_positive");
    }
}
