using EShop.Application.Abstractions.Mappers;
using EShop.Domain.Brands;

namespace EShop.Application.Brands.Queries.GetAllBrands;

public sealed record GetAllBrandsQuery
    : ICachedQuery<List<BrandResponse>>
{
    public string CachKey => "brands-all";

    public TimeSpan? Period => null;
}

internal sealed class GetAllBrandsQueryHandler(IBrandRepository brandRepository, Mapper mapper, ISupabaseService supabaseService)
        : IQueryHandler<GetAllBrandsQuery, List<BrandResponse>>
{
    public async Task<Result<List<BrandResponse>>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await brandRepository.GetAllAsync(cancellationToken);

        List<BrandResponse> response = new();

        foreach (var brand in brands)
        {
            BrandResponse brandResponse = mapper.MapToBrandResponse(brand);
            brandResponse.Image = supabaseService.GetPublicUrl("Brands", brand.Image);
            response.Add(
            brandResponse);
        }
        return response;    
    }
}