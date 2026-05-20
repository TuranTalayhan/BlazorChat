using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;

namespace BlazorChat.Server.Hubs;

public interface IServerHubClient
{
    Task ServerUpdated(ServerDto server);
    Task ChannelCreated(int? serverId, ChannelDto channel);
    Task ChannelDeleted(int? serverId, int channelId);
    Task CategoryCreated(int? serverId, CategoryDto category);
    Task CategoryDeleted(int? serverId, int categoryId);
    Task UserRoleUpdated(int serverId, int userId, ServerRole newRole);
}