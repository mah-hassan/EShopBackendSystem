using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Infrastructure.Data.Configurations;

public class CategoryBrandsConfigurations
    : IEntityTypeConfiguration<CategoryBrands>
{
    public void Configure(EntityTypeBuilder<CategoryBrands> builder)
    {
        builder.HasKey(cb => new { cb.CategoryId, cb.BrandId });
    }
}
