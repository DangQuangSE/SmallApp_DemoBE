using SecondBike.Domain.Enums;

namespace SecondBike.Application.DTOs.Inspections;

public class CreateInspectionDto
{
    public Guid BikePostId { get; set; }
    public OverallCondition OverallCondition { get; set; }
    public decimal? EstimatedValue { get; set; }
    public bool IsRecommended { get; set; }
    public string Summary { get; set; } = string.Empty;
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
}

public class InspectionReportDto
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public InspectionStatus Status { get; set; }
    public OverallCondition OverallCondition { get; set; }
    public decimal? EstimatedValue { get; set; }
    public bool IsRecommended { get; set; }
    public string Summary { get; set; } = string.Empty;
    public int FrameScore { get; set; }
    public int BrakesScore { get; set; }
    public int GearsScore { get; set; }
    public int WheelsScore { get; set; }
    public int TiresScore { get; set; }
    public int ChainScore { get; set; }
    public bool HasFrameDamage { get; set; }
    public bool HasRust { get; set; }
    public bool HasCracks { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public DateTime? InspectedAt { get; set; }
    public string BikeTitle { get; set; } = string.Empty;
}
