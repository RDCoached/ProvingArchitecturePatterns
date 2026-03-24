using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnionArch.Domain.Entities;
using OnionArch.Domain.ValueObjects;

namespace OnionArch.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.Property<int>("_id")
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.HasKey("_id");

        builder.Property(oi => oi.ProductId)
            .HasConversion(
                id => id.Value,
                value => ProductId.From(value))
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .HasConversion(
                q => q.Value,
                value => Quantity.Create(value))
            .HasColumnName("Quantity")
            .IsRequired();

        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(oi => oi.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}
