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
    private int GetUserId() =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    private async Task<bool> CanAccessChannel(int userId, int channelId)
    {
        var channel = await db.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null) return false;

        if (channel.Type == ChannelType.ServerText)
        {
            return await db.ServerMemberships
                .AnyAsync(sm => sm.UserId == userId && sm.ServerId == channel.ServerId);
        }

        return channel.Type == ChannelType.DirectMessage &&
               // Must be exactly one of the members in this DM
               channel.Members.Any(m => m.Id == userId);
    }
    
    [HttpGet("{channelId:int}")]
    public async Task<IActionResult> GetMessages(int channelId, [FromQuery] int count = 50)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();
        
        if (!await CanAccessChannel(userId, channelId)) return Forbid();
        
        var messages = await db.Messages
            .Include(m => m.Author)
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Type = (Shared.DTO.MessageType)m.Type,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                AuthorId = m.AuthorId,
                AuthorUsername = m.Author.Username,
                AuthorAvatarUrl = m.Author.AvatarUrl,
                ChannelId = m.ChannelId
            })
            .ToListAsync();

        messages.Reverse();
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        if (dto.ChannelId <= 0) return BadRequest("Invalid channel ID.");
        
        if (!await CanAccessChannel(userId, dto.ChannelId)) return Forbid();

        var user = await db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();
        
        var message = new Message
        {
            Content = dto.Content.Trim(),
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
            AuthorUsername = user.Username,
            AuthorAvatarUrl = user.AvatarUrl,
            ChannelId = message.ChannelId
        };
        
        var groupName = $"channel:{dto.ChannelId}";
        await hub.Clients.Group(groupName).ReceiveMessage(messageDto);
        
        return Ok(messageDto);
    }
}

