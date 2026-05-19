using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Interfaces.Repositories;

public interface IFriendshipRepository
{
    Task<List<FriendshipDto>> GetAcceptedFriendsAsync(int userId, CancellationToken ct);
}