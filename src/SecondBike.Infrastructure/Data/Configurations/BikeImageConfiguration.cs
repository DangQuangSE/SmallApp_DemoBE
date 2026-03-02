using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class BikeImageConfiguration : IEntityTypeConfiguration<BikeImage>
{
    public void Configure(EntityTypeBuilder<BikeImage> builder)
    {
        builder.ToTable("BikeImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        builder.Property(x => x.Caption).HasMaxLength(200);

        builder.HasOne(x => x.BikePost)
            .WithMany(p => p.Images)
            .HasForeignKey(x => x.BikePostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
