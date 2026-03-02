using SecondBike.Domain.Common;
using SecondBike.Domain.Enums;

namespace SecondBike.Domain.Entities;

/// <summary>
/// Chat message between two users, optionally linked to a bike post.
/// </summary>
public class Message : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid? BikePostId { get; set; }

    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public string? AttachmentUrl { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual AppUser Sender { get; set; } = null!;
    public virtual AppUser Receiver { get; set; } = null!;
    public virtual BikePost? BikePost { get; set; }
}
