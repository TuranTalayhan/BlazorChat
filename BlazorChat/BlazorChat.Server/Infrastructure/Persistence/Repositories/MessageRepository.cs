using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using MessageType = BlazorChat.Shared.DTO.MessageType;

namespace BlazorChat.Server.Infrastructure.Persistence.Repositories;

public class MessageRepository(AppDbContext db) : IMessageRepository
{
    public async Task<List<ChannelUnreadStatusDto>> GetUnreadStatusesForUserAsync(int userId, CancellationToken ct)
    {
        return await db.Channels
            .Select(c => new
            {
                ChannelId = c.Id,
                LatestMessageId = c.Messages.Max(m => (int?)m.Id) ?? 0,
                LastReadId = db.UserChannelStates
                    .Where(ucs => ucs.UserId == userId && ucs.ChannelId == c.Id)
                    .Select(ucs => ucs.LastReadMessageId)
                    .FirstOrDefault()
            })
            .Select(x => new ChannelUnreadStatusDto
            {
                ChannelId = x.ChannelId,
                HasUnreadMessages = x.LatestMessageId > x.LastReadId
            })
            .ToListAsync(ct);
    }
    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken ct)
    {
        return await db.Users.FindAsync([userId], ct);
    }

    public async Task<int> GetDmRecipientIdAsync(int channelId, int senderUserId, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Where(c => c.Id == channelId)
            .SelectMany(c => c.Members)
            .Where(m => m.Id != senderUserId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Message message, CancellationToken ct)
    {
        await db.Messages.AddAsync(message, ct);
    }
    
    public async Task<bool> ChannelExistsAsync(int channelId, CancellationToken ct)
    {
        return await db.Channels.AnyAsync(c => c.Id == channelId, ct);
    }

    public async Task<bool> MessageExistsInChannelAsync(int messageId, int channelId, CancellationToken ct)
    {
        return await db.Messages.AnyAsync(m => m.Id == messageId && m.ChannelId == channelId, ct);
    }

    public async Task<UserChannelState?> GetUserChannelStateAsync(int userId, int channelId, CancellationToken ct)
    {
        return await db.UserChannelStates
            .FirstOrDefaultAsync(ucs => ucs.UserId == userId && ucs.ChannelId == channelId, ct);
    }

    public async Task AddUserChannelStateAsync(UserChannelState state, CancellationToken ct)
    {
        await db.UserChannelStates.AddAsync(state, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
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