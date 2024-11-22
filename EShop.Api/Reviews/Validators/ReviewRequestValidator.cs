using EShop.Contracts.Reviews;
using FluentValidation;

namespace EShop.Api.Reviews.Validators;

public sealed class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{
    public ReviewRequestValidator()
    {
        RuleFor(r => r.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(r => r.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(r => r.Comment)
            .MaximumLength(500).WithMessage("Comment must not exceed 500 characters")
            .Matches(@"^[A-Za-z0-9\s\-_,\.;:!()]*$").WithMessage("Comment contains invalid characters");
    }
}
