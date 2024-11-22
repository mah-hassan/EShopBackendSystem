using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Extensions;
using EShop.Contracts.Reviews;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Reviews.Commands.UpdateReview;

public sealed record UpdateReviewCommand(Guid Id, ReviewRequest UpdatedReview)
    : ICommand<ReviewResponse>;
internal sealed class UpdateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor contextAccessor,
    Mapper mapper)
    : ICommandHandler<UpdateReviewCommand, ReviewResponse>
{
    public async Task<Result<ReviewResponse>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();
        var review = await reviewRepository.GetByIdAsync(request.Id);
        if (review is null)
        {
            return Result.Failure<ReviewResponse>(new Error("Review", "Review not found", ErrorType.NotFound));
        }
        if (review.UserId != userId)
        {
            return Result.Failure<ReviewResponse>(new Error("Review", "You are not allawed to access this resource", ErrorType.Forbidden));
        }

        review.Rating = request.UpdatedReview.Rating;
        review.Comment = request.UpdatedReview.Comment;

        reviewRepository.Update(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ReviewResponse reviewResponse = mapper.MapToReviewResponse(review);
        //TODO: add user information 
        return reviewResponse;
    }
}