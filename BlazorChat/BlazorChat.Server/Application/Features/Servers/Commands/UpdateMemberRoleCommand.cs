using BlazorChat.Server.Application.Features.Channels;
using BlazorChat.Shared.Enums;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Commands;

public record UpdateMemberRoleCommand(
    int ServerId, 
    int TargetUserId, 
    ServerRole NewRole, 
    int CurrentUserId
) : ICommand<ChannelResult<bool>>;