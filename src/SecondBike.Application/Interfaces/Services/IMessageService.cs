using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Chat;

namespace SecondBike.Application.Interfaces.Services;

/// <summary>
/// Service for messaging (Interaction — Team Member 3).
/// </summary>
public interface IMessageService
{
    Task<Result<MessageDto>> SendAsync(Guid senderId, SendMessageDto dto, CancellationToken ct = default);
    Task<Result<List<MessageDto>>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken ct = default);
    Task<Result<List<ConversationDto>>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(Guid userId, Guid otherUserId, CancellationToken ct = default);
    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
}
