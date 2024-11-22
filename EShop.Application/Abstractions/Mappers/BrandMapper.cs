using Riok.Mapperly.Abstractions;
using EShop.Domain.Brands;

namespace EShop.Application.Abstractions.Mappers;

public partial class Mapper
{
    [MapperIgnoreTarget(nameof(BrandResponse.Image))]
    public partial BrandResponse MapToBrandResponse(Brand brand);

    [MapperIgnoreTarget(nameof(Brand.Image))]
    public partial Brand MapToBrand(BrandRequest dto);

}