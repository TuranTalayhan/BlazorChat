using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<List<ChannelUnreadStatusDto>> GetUnreadStatusesForUserAsync(int userId, CancellationToken ct);   
    Task<User?> GetUserByIdAsync(int userId, CancellationToken ct);
    Task<int> GetDmRecipientIdAsync(int channelId, int senderUserId, CancellationToken ct);
    Task AddAsync(Message message, CancellationToken ct);
    Task<bool> ChannelExistsAsync(int channelId, CancellationToken ct);
    Task<bool> MessageExistsInChannelAsync(int messageId, int channelId, CancellationToken ct);
    Task<UserChannelState?> GetUserChannelStateAsync(int userId, int channelId, CancellationToken ct);
    Task AddUserChannelStateAsync(UserChannelState state, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<List<MessageDto>> GetPagedMessagesAsync(
        int channelId, DateTime? before, int? exclusiveId, int count, CancellationToken ct);
}