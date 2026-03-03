namespace SecondBike.Application.DTOs.Chat;

public class SendMessageDto
{
    public int ReceiverId { get; set; }
    public int? ListingId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class MessageDto
{
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool? IsRead { get; set; }
    public DateTime? SentAt { get; set; }
}

public class ConversationDto
{
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
