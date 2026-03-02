using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecondBike.Domain.Entities;

namespace SecondBike.Infrastructure.Data.Configurations;

public class InspectionReportConfiguration : IEntityTypeConfiguration<InspectionReport>
{
    public void Configure(EntityTypeBuilder<InspectionReport> builder)
    {
        builder.ToTable("InspectionReports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReportNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ReportNumber).IsUnique();
        builder.Property(x => x.Summary).HasMaxLength(3000);
        builder.Property(x => x.FrameNotes).HasMaxLength(1000);
        builder.Property(x => x.ReplacedComponents).HasMaxLength(1000);
        builder.Property(x => x.EstimatedValue).HasPrecision(18, 2);

        builder.HasIndex(x => x.BikePostId).IsUnique();

        builder.HasOne(x => x.BikePost)
            .WithOne(p => p.InspectionReport)
            .HasForeignKey<InspectionReport>(x => x.BikePostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Inspector)
            .WithMany(u => u.InspectionReports)
            .HasForeignKey(x => x.InspectorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
