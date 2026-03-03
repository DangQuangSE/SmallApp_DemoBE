using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time chat.
/// Requires JWT authentication — token passed via query string ?access_token=...
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var senderId = GetUserId();
        if (senderId == 0) return;

        var result = await _messageService.SendAsync(senderId, dto);
        if (result.IsSuccess && result.Data != null)
        {
            await Clients.Group(dto.ReceiverId.ToString()).SendAsync("ReceiveMessage", result.Data);
            await Clients.Group(senderId.ToString()).SendAsync("ReceiveMessage", result.Data);
        }
    }

    public async Task MarkAsRead(int otherUserId)
    {
        var userId = GetUserId();
        if (userId == 0) return;

        await _messageService.MarkAsReadAsync(userId, otherUserId);
        await Clients.Group(otherUserId.ToString()).SendAsync("MessagesRead", userId);
    }

    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
