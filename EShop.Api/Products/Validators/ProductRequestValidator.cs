using EShop.Contracts.Products;
using FluentValidation;

namespace EShop.Api.Products.Validators;

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters")
            .Matches(@"^[A-Za-z0-9\s\-_]+$").WithMessage("Name contains invalid characters");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(p => p.PrimaryImage)
            .NotNull().WithMessage("PrimaryImage is required");

        RuleFor(p => p.Images)
            .Must(images => images != null && images.Count > 0).WithMessage("At least one additional image is required");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("StockQuantity must be a non-negative value");

        RuleFor(p => p.Price)
            .NotNull().WithMessage("Price is required")
            .SetValidator(new MoneyDtoValidator());

        RuleFor(p => p.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .Matches(@"^[A-Z0-9]+$").WithMessage("SKU must consist of uppercase letters and digits only");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(p => p.BrandId)
            .NotEmpty().WithMessage("BrandId is required");

        RuleFor(p => p.Attribuates)
            .NotEmpty().WithMessage("At least one attribute is required")
            .Must(attribs => attribs.All(id => id != Guid.Empty)).WithMessage("Invalid attribute Ids found");
    }
}