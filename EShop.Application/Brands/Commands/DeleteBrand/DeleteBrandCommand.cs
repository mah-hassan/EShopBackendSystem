using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid brandId)
    : ICommand;
internal sealed class DeleteBrandCommandHandler(
    IBrandRepository brandRepository,
    IEventBus eventBus,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBrandCommand>
{
    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.brandId);
        if (brand is null || brand.IsDeleted)
        {
            return Result.Failure(new Error("Brand", "Brand not found", ErrorType.NotFound));
        }
        brand.IsDeleted = true;
        brandRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventBus.PublishAsync(new BrandDeletedEvent(brand.Id));
        return Result.Success();    
    }
}