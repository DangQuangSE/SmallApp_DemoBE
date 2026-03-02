using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.BikePostId }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.Wishlists)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BikePost)
            .WithMany(p => p.Wishlists)
            .HasForeignKey(x => x.BikePostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
