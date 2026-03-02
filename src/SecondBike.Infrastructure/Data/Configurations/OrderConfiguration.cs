using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.Property(x => x.BikePrice).HasPrecision(18, 2);
        builder.Property(x => x.DepositAmount).HasPrecision(18, 2);
        builder.Property(x => x.DepositPercentage).HasPrecision(5, 2);
        builder.Property(x => x.RemainingAmount).HasPrecision(18, 2);
        builder.Property(x => x.ShippingFee).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);
        builder.Property(x => x.TrackingNumber).HasMaxLength(100);
        builder.Property(x => x.ShippingProvider).HasMaxLength(100);
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);
        builder.Property(x => x.DisputeReason).HasMaxLength(2000);
        builder.Property(x => x.DisputeResolution).HasMaxLength(2000);

        builder.HasOne(x => x.Buyer)
            .WithMany(u => u.OrdersAsBuyer)
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Seller)
            .WithMany(u => u.OrdersAsSeller)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BikePost)
            .WithMany(p => p.Orders)
            .HasForeignKey(x => x.BikePostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
