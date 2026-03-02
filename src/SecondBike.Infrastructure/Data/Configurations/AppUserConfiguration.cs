using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.ShopName).HasMaxLength(200);
        builder.Property(x => x.ShopDescription).HasMaxLength(2000);
        builder.Property(x => x.SellerRating).HasPrecision(3, 2);
        builder.Property(x => x.IdentityUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => x.IdentityUserId).IsUnique();
    }
}
