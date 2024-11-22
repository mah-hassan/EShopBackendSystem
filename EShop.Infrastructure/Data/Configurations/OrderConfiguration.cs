using EShop.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace EShop.Infrastructure.Data.Configurations;

internal sealed class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ComplexProperty(o => o.ShippingInfo);

        builder.HasMany(o => o.Items)
            .WithOne()
            .IsRequired();

        builder.HasOne(o => o.DeliveryMethod)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(o => o.Status)
            .HasConversion(s => s.ToString(),
            v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));

        builder.Property(o => o.CustomerEmail)
            .HasMaxLength(150);

        builder.HasIndex(o => o.CustomerEmail);

        builder.Navigation(o => o.Items).AutoInclude();
        builder.Navigation(o => o.DeliveryMethod).AutoInclude();
    }
}
internal sealed class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(oi => oi.Variants)
            .HasConversion(oi => JsonConvert.SerializeObject(oi),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new());
        builder.ComplexProperty(oi => oi.UnitPrice, moneyBuilder =>
        {
            moneyBuilder
                .Property(m => m.Ammount)
                .HasPrecision(8, 2)
                .IsRequired();

            moneyBuilder
                .Property(m => m.Currency)
                .HasMaxLength(4)
                .IsRequired();
        });
    }
}
internal sealed class DeliveryMethodConfiguration
    : IEntityTypeConfiguration<DeliveryMethod>
{
    public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
    {
        builder.Property(dm => dm.DeliveryCost)
            .HasPrecision(8, 2);
    }
}