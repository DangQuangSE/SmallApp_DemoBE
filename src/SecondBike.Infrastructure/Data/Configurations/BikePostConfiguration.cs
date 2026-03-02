using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class BikePostConfiguration : IEntityTypeConfiguration<BikePost>
{
    public void Configure(EntityTypeBuilder<BikePost> builder)
    {
        builder.ToTable("BikePosts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(5000).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Brand).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FrameMaterial).HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(50);
        builder.Property(x => x.WeightKg).HasPrecision(5, 2);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.ModerationNotes).HasMaxLength(1000);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.Property(x => x.UsageHistory).HasMaxLength(2000);
        builder.Property(x => x.AccidentDescription).HasMaxLength(2000);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Category, x.Status });
        builder.HasIndex(x => new { x.Brand, x.Status });

        builder.HasOne(x => x.Seller)
            .WithMany(u => u.BikePosts)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
