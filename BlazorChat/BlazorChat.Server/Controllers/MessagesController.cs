using System.Security.Claims;
using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController(AppDbContext db, IHubContext<ChatHub, IChatClient> hub) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] int channelId = 1, [FromQuery] int count = 50)
    {
        var messages = await db.Messages
            .Include(m => m.Author)
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                AuthorId = m.AuthorId,
                AuthorUsername = m.Author.Username,
                ChannelId = m.ChannelId
            })
            .ToListAsync();

        messages.Reverse();
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        var message = new Message
        {
            Content = dto.Content,
            ChannelId = dto.ChannelId,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        var messageDto = new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            AuthorId = userId,
            AuthorUsername = User.Identity?.Name ?? "",
            ChannelId = message.ChannelId
        };

        await hub.Clients.All.ReceiveMessage(messageDto);
        return Ok(messageDto);
    }
}
