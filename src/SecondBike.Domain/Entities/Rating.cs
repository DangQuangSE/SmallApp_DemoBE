using SecondBike.Domain.Common;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Buyer-to-seller rating after order completion.
/// </summary>
public class Rating : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }

    public int Stars { get; set; } // 1-5
    public string? Comment { get; set; }

    public int? CommunicationRating { get; set; }
    public int? AccuracyRating { get; set; }
    public int? PackagingRating { get; set; }
    public int? SpeedRating { get; set; }

    public bool IsPublic { get; set; } = true;

    public string? SellerResponse { get; set; }
    public DateTime? SellerRespondedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual AppUser FromUser { get; set; } = null!;
    public virtual AppUser ToUser { get; set; } = null!;
}
