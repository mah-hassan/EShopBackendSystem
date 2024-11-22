using EShop.Application.Abstractions.Mappers;
using EShop.Application.Common.Constants;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Brands.Commands.AddBrand;

public sealed record CreateBrandCommand(BrandRequest dto)
    : ICommand<BrandResponse>;
internal sealed class CreateBrandCommandHandler(
    Mapper mapper,
    ISupabaseService supabaseService,
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<Result<BrandResponse>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (await brandRepository.IsNameExists(request.dto.Name))
        {
            return Result.Failure<BrandResponse>(new Error("Brand", "Brand Already Exsists", ErrorType.Conflict));
        }
        var brand = mapper.MapToBrand(request.dto);

        if(request.dto.Image is not null)
        {
            string supabasePath = $"Brand-{brand.Id}{Path.GetExtension(request.dto.Image.FileName)}";
            brand.Image = await supabaseService.UploadAsync(request.dto.Image, "Brands", supabasePath);
        }

        brandRepository.Add(brand);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = mapper.MapToBrandResponse(brand);
        response.Image = supabaseService.GetPublicUrl(SupabaseBackets.Brands, brand.Image);
        return response;
    }
}
