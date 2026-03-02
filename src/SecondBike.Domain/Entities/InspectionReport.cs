using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Vehicle inspection report created by a certified inspector.
/// </summary>
public class InspectionReport : BaseEntity
{
    public Guid BikePostId { get; set; }
    public Guid InspectorId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;

    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;
    public OverallCondition OverallCondition { get; set; }
    public decimal? EstimatedValue { get; set; }
    public bool IsRecommended { get; set; }
    public string Summary { get; set; } = string.Empty;

    // Component scores (1-10)
    public int FrameScore { get; set; }
    public int BrakesScore { get; set; }
    public int GearsScore { get; set; }
    public int WheelsScore { get; set; }
    public int TiresScore { get; set; }
    public int ChainScore { get; set; }

    public bool HasFrameDamage { get; set; }
    public string? FrameNotes { get; set; }
    public bool HasRust { get; set; }
    public bool HasCracks { get; set; }
    public bool AllComponentsOriginal { get; set; }
    public string? ReplacedComponents { get; set; }

    public DateTime? InspectedAt { get; set; }

    public virtual BikePost BikePost { get; set; } = null!;
    public virtual AppUser Inspector { get; set; } = null!;
}
