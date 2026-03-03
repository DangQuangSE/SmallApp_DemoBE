using Microsoft.AspNetCore.SignalR;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time chat.
/// </summary>
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task JoinChat(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task SendMessage(string senderId, SendMessageDto dto)
    {
        if (!int.TryParse(senderId, out var sId)) return;

        var result = await _messageService.SendAsync(sId, dto);
        if (result.IsSuccess && result.Data != null)
        {
            await Clients.Group(dto.ReceiverId.ToString()).SendAsync("ReceiveMessage", result.Data);
            await Clients.Group(senderId).SendAsync("ReceiveMessage", result.Data);
        }
    }

    public async Task MarkAsRead(string userId, string otherUserId)
    {
        if (int.TryParse(userId, out var uId) && int.TryParse(otherUserId, out var oId))
        {
            await _messageService.MarkAsReadAsync(uId, oId);
            await Clients.Group(otherUserId).SendAsync("MessagesRead", userId);
        }
    }
}
