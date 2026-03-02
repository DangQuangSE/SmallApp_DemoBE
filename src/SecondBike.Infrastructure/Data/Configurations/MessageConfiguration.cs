using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.AttachmentUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.SenderId, x.ReceiverId });

        builder.HasOne(x => x.Sender)
            .WithMany(u => u.MessagesSent)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Receiver)
            .WithMany(u => u.MessagesReceived)
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BikePost)
            .WithMany()
            .HasForeignKey(x => x.BikePostId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
