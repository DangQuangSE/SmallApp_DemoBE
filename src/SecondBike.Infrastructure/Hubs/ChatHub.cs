using Microsoft.AspNetCore.SignalR;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time chat (Interaction & Community - Team Member 3).
/// </summary>
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>
    /// Joins a personal group to receive targeted messages.
    /// </summary>
    public async Task JoinChat(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    /// <summary>
    /// Sends a message to a specific user and saves it to the database.
    /// </summary>
    public async Task SendMessage(string senderId, SendMessageDto dto)
    {
        if (!Guid.TryParse(senderId, out var sId)) return;

        var result = await _messageService.SendAsync(sId, dto);
        if (result.IsSuccess && result.Data != null)
        {
            // Emit to receiver's personal group
            await Clients.Group(dto.ReceiverId.ToString()).SendAsync("ReceiveMessage", result.Data);
            
            // Emit to sender's personal group (for multi-device sync)
            await Clients.Group(senderId).SendAsync("ReceiveMessage", result.Data);
        }
    }

    /// <summary>
    /// Notifies the other user that messages are being read.
    /// </summary>
    public async Task MarkAsRead(string userId, string otherUserId)
    {
        if (Guid.TryParse(userId, out var uId) && Guid.TryParse(otherUserId, out var oId))
        {
            await _messageService.MarkAsReadAsync(uId, oId);
            await Clients.Group(otherUserId).SendAsync("MessagesRead", userId);
        }
    }
}
