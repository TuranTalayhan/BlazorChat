using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class MessageRepository(AppDbContext db) : IMessageRepository
{
    public async Task<List<MessageDto>> GetPagedMessagesAsync(
        int channelId, 
        DateTime? beforeTimestamp, 
        int? exclusiveMessageId, 
        int count, 
        CancellationToken ct)
    {
        var query = db.Messages.Where(m => m.ChannelId == channelId);

        if (beforeTimestamp.HasValue && exclusiveMessageId.HasValue)
        {
            var cursorTime = beforeTimestamp.Value;
            var cursorId = exclusiveMessageId.Value;

            query = query.Where(m => m.CreatedAt < cursorTime || 
                                     (m.CreatedAt == cursorTime && m.Id < cursorId));
        }
        else if (beforeTimestamp.HasValue)
        {
            query = query.Where(m => m.CreatedAt < beforeTimestamp.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(count)                 
            .Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                Type = (MessageType)m.Type,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                AuthorId = m.AuthorId,
                AuthorUsername = m.Author.Username,
                AuthorAvatarUrl = m.Author.AvatarUrl,
                ChannelId = m.ChannelId
            })
            .ToListAsync(ct);
    }
}