using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("Ratings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.Property(x => x.SellerResponse).HasMaxLength(1000);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasOne(x => x.Order)
            .WithOne(o => o.Rating)
            .HasForeignKey<Rating>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromUser)
            .WithMany(u => u.RatingsGiven)
            .HasForeignKey(x => x.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToUser)
            .WithMany(u => u.RatingsReceived)
            .HasForeignKey(x => x.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
