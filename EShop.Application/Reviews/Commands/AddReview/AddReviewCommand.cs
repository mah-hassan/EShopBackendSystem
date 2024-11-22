using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Extensions;
using EShop.Contracts.Reviews;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Reviews.Commands.AddReview;

public sealed record AddReviewCommand(ReviewRequest Review)
    : ICommand;

internal sealed class AddReviewCommandHandler(
    IReviewRepository reviewRepository, 
    IUnitOfWork unitOfWork,
    IProductRepository productRepository,
    IHttpContextAccessor contextAccessor,
    Mapper mapper)
    : ICommandHandler<AddReviewCommand>
{
    public async Task<Result> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();

        var product = await productRepository.GetByIdAsync(request.Review.ProductId);

        if (product is null)
        {
            return Result.Failure(new Error("Product", "Product not found", ErrorType.NotFound));
        }

        var review = mapper.MapToReview(request.Review);

        review.UserId = userId;

        reviewRepository.Add(review);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}