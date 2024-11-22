using EShop.Application.Abstractions.Mappers;
using EShop.Domain.Brands;
using EShop.Domain.Shared.Errors;

namespace EShop.Application.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(Guid id)
    : ICachedQuery<BrandResponse>
{
    public string CachKey => $"brands-{id}";

    public TimeSpan? Period => TimeSpan.FromMinutes(10);
}


internal sealed class GetBrandByIdQueryHandler(
    IBrandRepository brandRepository,
    ISupabaseService supabaseService,
    Mapper mapper)
    : IQueryHandler<GetBrandByIdQuery, BrandResponse>
{
    public async Task<Result<BrandResponse>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.id);

        if (brand is null)
        {
            return Result.Failure<BrandResponse>(new Error("Brand", "Brand Not Found", ErrorType.NotFound));
        }

        var response = mapper.MapToBrandResponse(brand);

        var publicUrl = supabaseService.GetPublicUrl("Brands", brand.Image);

        response.Image = publicUrl;

        return response;
    }
}