using System.Threading.Tasks;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Shared.Hubs;

public interface IChatClient
{
	Task UserStatusChanged(int userId, UserStatus status);
	Task ReceiveMessage(MessageDto message);
}