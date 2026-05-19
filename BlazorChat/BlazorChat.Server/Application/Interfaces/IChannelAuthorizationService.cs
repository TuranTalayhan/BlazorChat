using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Interfaces;

public interface IChannelAuthorizationService
{
    Task<bool> CanAccessChannelAsync(int userId, int channelId, CancellationToken ct = default);
}

public class ChannelAuthorizationService(AppDbContext db) : IChannelAuthorizationService
{
    public async Task<bool> CanAccessChannelAsync(int userId, int channelId, CancellationToken ct = default)
    {
        var channel = await db.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == channelId, ct);

        if (channel == null) return false;

        if (channel.Type == ChannelType.Server)
        {
            return await db.ServerMemberships
                .AnyAsync(sm => sm.UserId == userId && sm.ServerId == channel.ServerId, ct);
        }

        return channel.Type == ChannelType.DirectMessage &&
               channel.Members.Any(m => m.Id == userId);
    }
}