using EShop.Contracts.Products;
using FluentValidation;

namespace EShop.Api.Products.Validators;

public sealed class MoneyDtoValidator : AbstractValidator<MoneyDto>
{
    public MoneyDtoValidator()
    {
        RuleFor(m => m.Ammount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(m => m.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a valid 3-letter code")
            .Matches(@"^[A-Z]{3}$").WithMessage("Currency must consist of 3 uppercase letters");
    }
}