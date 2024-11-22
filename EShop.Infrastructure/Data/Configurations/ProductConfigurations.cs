using EShop.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
namespace EShop.Infrastructure.Data.Configurations;

internal sealed class ProductConfigurations
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.ComplexProperty(p => p.Price, monyBuilder =>
        {
            monyBuilder
                .Property(m => m.Ammount)
                .HasPrecision(8, 2)
                .IsRequired();

            monyBuilder
                .Property(m => m.Currency)
                .HasMaxLength(4)
                .IsRequired();
        });

        builder.Property(p => p.Images)
             .HasConversion(images => JsonConvert.SerializeObject(images),
             v => JsonConvert.DeserializeObject<List<string>>(v) ?? new());

        builder.HasIndex(p => p.Sku)
          .IsUnique(true);

        builder.HasMany(p => p.VariantOptions)
          .WithMany()
          .UsingEntity<ProductAttribuates>();

        builder.Navigation(p => p.VariantOptions)
            .AutoInclude();
    }
}

internal sealed class ProductAttribuatesConfiguration
    : IEntityTypeConfiguration<ProductAttribuates>
{
    public void Configure(EntityTypeBuilder<ProductAttribuates> builder)
    {
        builder.HasKey(pa => new { pa.ProductId, pa.VariantOptionId });

        builder
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.ClientCascade);  // Set delete behavior to NoAction

        builder
            .HasOne<VariantOption>()
            .WithMany()
            .HasForeignKey(pa => pa.VariantOptionId)
            .OnDelete(DeleteBehavior.ClientCascade);  // Set delete behavior to NoAction
    }
}