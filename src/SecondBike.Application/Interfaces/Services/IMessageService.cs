using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Chat;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for messaging (Interaction).
/// </summary>
public interface IMessageService
{
    Task<Result<MessageDto>> SendAsync(int senderId, SendMessageDto dto, CancellationToken ct = default);
    Task<Result<List<MessageDto>>> GetConversationAsync(int userId, int otherUserId, CancellationToken ct = default);
    Task<Result<List<ConversationDto>>> GetConversationsAsync(int userId, CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(int userId, int otherUserId, CancellationToken ct = default);
    Task<Result<int>> GetUnreadCountAsync(int userId, CancellationToken ct = default);
}
