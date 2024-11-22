
using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(Guid brandId, BrandRequest dto)
    : ICommand<BrandResponse>;



internal sealed class UpdateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    ISupabaseService supabaseService,
    Mapper mapper)
    : ICommandHandler<UpdateBrandCommand, BrandResponse>
{
    public async Task<Result<BrandResponse>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.brandId);
        if (brand is null)
        {
            return Result.Failure<BrandResponse>(new Error("Brand", "Brand not found", ErrorType.NotFound));
        }
        brand.Name = request.dto.Name;
        brand.Description = request.dto.Description;
        if (request.dto.Image is not null)
        {
            await supabaseService.DeleteFileAsync(SupabaseBackets.Brands, brand.Image);
            string supabasePath = $"Brand-{brand.Id}{Path.GetExtension(request.dto.Image.FileName)}";
            brand.Image = await supabaseService.UploadAsync(request.dto.Image, SupabaseBackets.Brands, supabasePath);
        }
        brandRepository.Update(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var response = mapper.MapToBrandResponse(brand);
        response.Image = supabaseService.GetPublicUrl(SupabaseBackets.Brands, brand.Image);
        return response;
    }
}