using SecondBike.Application.Common;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces;
using SecondBike.Application.Interfaces.Services;
using SecondBike.Domain.Entities;

namespace SecondBike.Application.Services;

/// <summary>
/// Interaction — Messaging between users via ChatSession/ChatMessage.
/// Business logic belongs in Application layer.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IRepository<ChatSession> _sessionRepo;
    private readonly IRepository<ChatMessage> _msgRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _uow;

    public MessageService(
        IRepository<ChatSession> sessionRepo,
        IRepository<ChatMessage> msgRepo,
        IRepository<User> userRepo,
        IUnitOfWork uow)
    {
        _sessionRepo = sessionRepo;
        _msgRepo = msgRepo;
        _userRepo = userRepo;
        _uow = uow;
    }

    public async Task<Result<MessageDto>> SendAsync(int senderId, SendMessageDto dto, CancellationToken ct = default)
    {
        var sender = await _userRepo.GetByIdAsync(senderId, ct);
        if (sender is null) return Result<MessageDto>.Failure("Sender not found");

        var receiver = await _userRepo.GetByIdAsync(dto.ReceiverId, ct);
        if (receiver is null) return Result<MessageDto>.Failure("Receiver not found");

        var sessions = await _sessionRepo.FindAsync(s =>
            (s.BuyerId == senderId && s.SellerId == dto.ReceiverId) ||
            (s.BuyerId == dto.ReceiverId && s.SellerId == senderId), ct);
        var session = sessions.FirstOrDefault();

        if (session is null)
        {
            session = new ChatSession
            {
                BuyerId = senderId,
                SellerId = dto.ReceiverId,
                ListingId = dto.ListingId,
                StartedAt = DateTime.UtcNow
            };
            await _sessionRepo.AddAsync(session, ct);
            await _uow.SaveChangesAsync(ct);
        }

        var message = new ChatMessage
        {
            SessionId = session.SessionId,
            SenderId = senderId,
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        await _msgRepo.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<MessageDto>.Success(new MessageDto
        {
            MessageId = message.MessageId,
            SenderId = senderId,
            SenderName = sender.Username,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content,
            IsRead = false,
            SentAt = message.SentAt
        });
    }

    public async Task<Result<List<MessageDto>>> GetConversationAsync(int userId, int otherUserId, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.FindAsync(s =>
            (s.BuyerId == userId && s.SellerId == otherUserId) ||
            (s.BuyerId == otherUserId && s.SellerId == userId), ct);

        var sessionIds = sessions.Select(s => s.SessionId).ToList();
        if (sessionIds.Count == 0)
            return Result<List<MessageDto>>.Success(new List<MessageDto>());

        var allMessages = new List<ChatMessage>();
        foreach (var sid in sessionIds)
        {
            var msgs = await _msgRepo.FindAsync(m => m.SessionId == sid, ct);
            allMessages.AddRange(msgs);
        }

        var userMap = new Dictionary<int, User>();
        foreach (var uid in new[] { userId, otherUserId })
        {
            var user = await _userRepo.GetByIdAsync(uid, ct);
            if (user is not null) userMap[uid] = user;
        }

        var dtos = allMessages.OrderBy(m => m.SentAt).Select(m =>
        {
            return new MessageDto
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                SenderName = userMap.GetValueOrDefault(m.SenderId)?.Username ?? "Unknown",
                ReceiverId = m.SenderId == userId ? otherUserId : userId,
                Content = m.Content ?? string.Empty,
                IsRead = m.IsRead,
                SentAt = m.SentAt
            };
        }).ToList();

        return Result<List<MessageDto>>.Success(dtos);
    }

    public async Task<Result<List<ConversationDto>>> GetConversationsAsync(int userId, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.FindAsync(s =>
            s.BuyerId == userId || s.SellerId == userId, ct);

        var conversations = new List<ConversationDto>();
        foreach (var session in sessions)
        {
            var otherUserId = session.BuyerId == userId ? session.SellerId : session.BuyerId;
            var otherUser = await _userRepo.GetByIdAsync(otherUserId, ct);

            var messages = await _msgRepo.FindAsync(m => m.SessionId == session.SessionId, ct);
            var lastMsg = messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unread = messages.Count(m => m.SenderId != userId && m.IsRead == false);

            conversations.Add(new ConversationDto
            {
                OtherUserId = otherUserId,
                OtherUserName = otherUser?.Username ?? "Unknown",
                LastMessage = lastMsg?.Content ?? string.Empty,
                LastMessageAt = lastMsg?.SentAt,
                UnreadCount = unread
            });
        }

        return Result<List<ConversationDto>>.Success(
            conversations.OrderByDescending(c => c.LastMessageAt).ToList());
    }

    public async Task<Result> MarkAsReadAsync(int userId, int otherUserId, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.FindAsync(s =>
            (s.BuyerId == userId && s.SellerId == otherUserId) ||
            (s.BuyerId == otherUserId && s.SellerId == userId), ct);

        foreach (var session in sessions)
        {
            var unread = await _msgRepo.FindAsync(m =>
                m.SessionId == session.SessionId && m.SenderId == otherUserId && m.IsRead == false, ct);

            foreach (var msg in unread)
            {
                msg.IsRead = true;
                _msgRepo.Update(msg);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<int>> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    {
        var sessions = await _sessionRepo.FindAsync(s =>
            s.BuyerId == userId || s.SellerId == userId, ct);

        int totalUnread = 0;
        foreach (var session in sessions)
        {
            var count = await _msgRepo.CountAsync(m =>
                m.SessionId == session.SessionId && m.SenderId != userId && m.IsRead == false, ct);
            totalUnread += count;
        }

        return Result<int>.Success(totalUnread);
    }
}
