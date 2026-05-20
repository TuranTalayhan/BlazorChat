using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

public interface IFriendClient
{
    Task ReceiveFriendRequest(PendingFriendshipDto pendingFriendship);
	
    Task ReceiveNewFriend(FriendshipDto friendship);
}