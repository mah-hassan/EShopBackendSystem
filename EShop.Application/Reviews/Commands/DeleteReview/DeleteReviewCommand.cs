
using EShop.Application.Common.Extensions;
using EShop.Domain.Products;
using EShop.Domain.Shared.Errors;
using Microsoft.AspNetCore.Http;

namespace EShop.Application.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand(Guid Id)
    : ICommand;

internal sealed class DeleteReviewCommandHandler(
    IHttpContextAccessor contextAccessor,
    IReviewRepository reviewRepository,
    IUnitOfWork unitOfWork) :
    ICommandHandler<DeleteReviewCommand>
{
    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = contextAccessor.GetUserId();
        var review = await reviewRepository.GetByIdAsync(request.Id);
        if (review is null)
        {
            return Result.Failure(new Error("Review", "Review not found", ErrorType.NotFound));
        }
        if (review.UserId != userId)
        {
            return Result.Failure(new Error("Review", "You are not allawed to access this resource", ErrorType.Forbidden));
        } 
        reviewRepository.Delete(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}