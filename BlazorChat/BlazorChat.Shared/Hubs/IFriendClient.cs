using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Hubs;

public interface IFriendClient
{
    Task ReceiveFriendRequest(PendingFriendshipDto pendingFriendship);
	
    Task ReceiveNewFriend(FriendshipDto friendship);
}