using SecondBike.Domain.Common;

namespace SecondBike.Domain.Entities;

/// <summary>
/// User's saved / favorite bike posts (wishlist).
/// </summary>
public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BikePostId { get; set; }
    public string? Notes { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public virtual BikePost BikePost { get; set; } = null!;
}
