using System.Threading.Tasks;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Hubs;

public interface IChatClient
{
	Task UserStatusChanged(ReceiveUserStatus updateStatusDto);
	Task ReceiveMessage(MessageDto message);
	
	Task SendFriendRequest(PendingFriendshipDto pendingFriendship);
	
	Task NewFriendAdded(FriendshipDto friendship);
}