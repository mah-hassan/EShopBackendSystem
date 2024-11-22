using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Infrastructure.Data.Configurations;

internal sealed class VariantConfigurations
: IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.HasMany(v => v.Options)
            .WithOne(o => o.Variant)
            .HasForeignKey(vo => vo.VariantId);

        builder.Navigation(v => v.Options)
            .AutoInclude();
    }
}
