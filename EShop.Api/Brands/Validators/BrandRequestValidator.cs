using EShop.Contracts.Brand;
using FluentValidation;

namespace EShop.Api.Brands.Validators;

public class BrandRequestValidator : AbstractValidator<BrandRequest>
{
    public BrandRequestValidator()
    {
        RuleFor(b => b.Name).NotEmpty()
            .MaximumLength(300)
            .Matches(@"^[A-Za-z0-9\s\-_]*$");

        RuleFor(b => b.Image)
            .NotEmpty().WithMessage("Beand Logo Is Required");

    }
}