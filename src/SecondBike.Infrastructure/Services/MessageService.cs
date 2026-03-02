using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;
using SecondBike.Domain.Enums;

namespace SecondBike.Infrastructure.Services;

/// <summary>
/// Interaction — Messaging between users.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IRepository<Message> _msgRepo;
    private readonly IRepository<AppUser> _userRepo;
    private readonly IUnitOfWork _uow;

    public MessageService(
        IRepository<Message> msgRepo,
        IRepository<AppUser> userRepo,
        IUnitOfWork uow)
    {
        _msgRepo = msgRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<MessageDto>> SendAsync(Guid senderId, SendMessageDto dto, CancellationToken ct = default)
    {
        var sender = await _userRepo.GetByIdAsync(senderId, ct);
        if (sender is null) return Result<MessageDto>.Failure("Sender not found");

        var receiver = await _userRepo.GetByIdAsync(dto.ReceiverId, ct);
        if (receiver is null) return Result<MessageDto>.Failure("Receiver not found");

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            BikePostId = dto.BikePostId,
            Content = dto.Content,
            Type = MessageType.Text
        };

        await _msgRepo.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<MessageDto>.Success(new MessageDto
        {
            Id = message.Id,
            SenderId = senderId,
            SenderName = sender.FullName,
            SenderAvatar = sender.AvatarUrl,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content,
            IsRead = false,
            CreatedAt = message.CreatedAt
        });
    }

    public async Task<Result<List<MessageDto>>> GetConversationAsync(Guid userId, Guid otherUserId, CancellationToken ct = default)
    {
        var messages = await _msgRepo.FindAsync(m =>
            (m.SenderId == userId && m.ReceiverId == otherUserId) ||
            (m.SenderId == otherUserId && m.ReceiverId == userId), ct);

        var userMap = new Dictionary<Guid, AppUser>();
        foreach (var uid in new[] { userId, otherUserId })
        {
            var user = await _userRepo.GetByIdAsync(uid, ct);
            if (user is not null) userMap[uid] = user;
        }

        var dtos = messages.OrderBy(m => m.CreatedAt).Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = userMap.GetValueOrDefault(m.SenderId)?.FullName ?? "Unknown",
            SenderAvatar = userMap.GetValueOrDefault(m.SenderId)?.AvatarUrl,
            ReceiverId = m.ReceiverId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();

        return Result<List<MessageDto>>.Success(dtos);
    }

    public async Task<Result<List<ConversationDto>>> GetConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        var allMessages = await _msgRepo.FindAsync(m =>
            m.SenderId == userId || m.ReceiverId == userId, ct);

        var groups = allMessages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var lastMsg = g.OrderByDescending(m => m.CreatedAt).First();
                return new
                {
                    OtherUserId = g.Key,
                    LastMessage = lastMsg.Content,
                    LastMessageAt = lastMsg.CreatedAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();

        var conversations = new List<ConversationDto>();
        foreach (var g in groups)
        {
            var otherUser = await _userRepo.GetByIdAsync(g.OtherUserId, ct);
            conversations.Add(new ConversationDto
            {
                OtherUserId = g.OtherUserId,
                OtherUserName = otherUser?.FullName ?? "Unknown",
                OtherUserAvatar = otherUser?.AvatarUrl,
                LastMessage = g.LastMessage,
                LastMessageAt = g.LastMessageAt,
                UnreadCount = g.UnreadCount
            });
        }

        return Result<List<ConversationDto>>.Success(conversations);
    }

    public async Task<Result> MarkAsReadAsync(Guid userId, Guid otherUserId, CancellationToken ct = default)
    {
        var unread = await _msgRepo.FindAsync(m =>
            m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead, ct);

        foreach (var msg in unread)
        {
            msg.IsRead = true;
            msg.ReadAt = DateTime.UtcNow;
            _msgRepo.Update(msg);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var count = await _msgRepo.CountAsync(m => m.ReceiverId == userId && !m.IsRead, ct);
        return Result<int>.Success(count);
    }
}
