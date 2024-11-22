using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Infrastructure.Data.Configurations;

internal sealed class CategoryConfigurations
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId);

        builder.Property(c => c.ParentCategoryId)
            .HasDefaultValue(Guid.Empty);
     
        builder.HasIndex(c => c.ParentCategoryId);
        builder.Property(c => c.IsParentCategory)
            .HasDefaultValue(false);

        builder.HasMany(c => c.Variants)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
