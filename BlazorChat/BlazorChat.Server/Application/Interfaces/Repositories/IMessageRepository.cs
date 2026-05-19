using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<List<MessageDto>> GetPagedMessagesAsync(
        int channelId, DateTime? before, int? exclusiveId, int count, CancellationToken ct);
}