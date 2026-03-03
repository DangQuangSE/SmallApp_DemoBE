using System;
using System.Collections.Generic;

namespace SecondBike.Domain.Entities;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    public int SessionId { get; set; }

    public int SenderId { get; set; }

    public string? Content { get; set; }

    public DateTime? SentAt { get; set; }

    public bool? IsRead { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual ChatSession Session { get; set; } = null!;
}
