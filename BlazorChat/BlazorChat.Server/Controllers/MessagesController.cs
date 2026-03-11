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

    [HttpGet]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int? channelId,
        [FromQuery] int? directMessageId,
        [FromQuery] int count = 50)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        if (channelId.HasValue)
        {
            // Verify caller is a member of this channel's server
            var isMember = await db.ServerMemberships
                .AnyAsync(sm => sm.UserId == userId &&
                                db.Channels.Any(c => c.Id == channelId && c.ServerId == sm.ServerId));
            if (!isMember) return Forbid();

            var messages = await db.Messages
                .Include(m => m.Author)
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Type = (BlazorChat.Shared.DTO.MessageType)m.Type,
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

        if (directMessageId.HasValue)
        {
            // Verify caller is a participant in this DM
            var isDmParticipant = await db.DirectMessages
                .AnyAsync(dm => dm.Id == directMessageId &&
                               (dm.User1Id == userId || dm.User2Id == userId));
            if (!isDmParticipant) return Forbid();

            var messages = await db.Messages
                .Include(m => m.Author)
                .Where(m => m.DirectMessageId == directMessageId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Type = (BlazorChat.Shared.DTO.MessageType)m.Type,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    AuthorId = m.AuthorId,
                    AuthorUsername = m.Author.Username,
                    AuthorAvatarUrl = m.Author.AvatarUrl,
                    DirectMessageId = m.DirectMessageId
                })
                .ToListAsync();

            messages.Reverse();
            return Ok(messages);
        }

        return BadRequest("Provide channelId or directMessageId.");
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        if (dto.ChannelId == null && dto.DirectMessageId == null)
            return BadRequest("Provide channelId or directMessageId.");

        string groupName;

        if (dto.ChannelId.HasValue)
        {
            var isMember = await db.ServerMemberships
                .AnyAsync(sm => sm.UserId == userId &&
                                db.Channels.Any(c => c.Id == dto.ChannelId && c.ServerId == sm.ServerId));
            if (!isMember) return Forbid();
            groupName = $"channel:{dto.ChannelId}";
        }
        else
        {
            var isDmParticipant = await db.DirectMessages
                .AnyAsync(dm => dm.Id == dto.DirectMessageId &&
                               (dm.User1Id == userId || dm.User2Id == userId));
            if (!isDmParticipant) return Forbid();
            groupName = $"dm:{dto.DirectMessageId}";
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var message = new Message
        {
            Content = dto.Content.Trim(),
            ChannelId = dto.ChannelId,
            DirectMessageId = dto.DirectMessageId,
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
            ChannelId = message.ChannelId,
            DirectMessageId = message.DirectMessageId
        };

        await hub.Clients.Group(groupName).ReceiveMessage(messageDto);
        return Ok(messageDto);
    }
}

