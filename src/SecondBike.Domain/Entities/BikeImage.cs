using SecondBike.Domain.Common;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Image attached to a bike post.
/// </summary>
public class BikeImage : BaseEntity
{
    public Guid BikePostId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public string? Caption { get; set; }

    public virtual BikePost BikePost { get; set; } = null!;
}
