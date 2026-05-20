using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Hubs;

public interface IChatClient
{
	Task ReceiveMessage(MessageDto message);
	
	Task ReadStateUpdated(int channelId, int lastMessageId);
}