using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Chat;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

/// <summary>
/// REST endpoints for messaging. Real-time delivery is handled by the SignalR ChatHub.
/// </summary>
[Authorize]
public class MessagesController : BaseApiController
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto, CancellationToken ct)
        => ToResponse(await _messageService.SendAsync(GetCurrentUserId(), dto, ct));

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
        => ToResponse(await _messageService.GetConversationsAsync(GetCurrentUserId(), ct));

    [HttpGet("conversations/{otherUserId:guid}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId, CancellationToken ct)
        => ToResponse(await _messageService.GetConversationAsync(GetCurrentUserId(), otherUserId, ct));

    [HttpPatch("conversations/{otherUserId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid otherUserId, CancellationToken ct)
        => ToResponse(await _messageService.MarkAsReadAsync(GetCurrentUserId(), otherUserId, ct));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        => ToResponse(await _messageService.GetUnreadCountAsync(GetCurrentUserId(), ct));
}
