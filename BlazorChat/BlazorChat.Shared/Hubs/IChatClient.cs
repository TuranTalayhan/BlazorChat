using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Hubs;

public interface IChatClient
{
	Task ReceiveMessage(MessageDto message);
	
	Task ReadStateUpdated(int channelId, int lastMessageId);
}