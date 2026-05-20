using BlazorChat.Server.Application.Features.Channels;
using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Hubs;
using BlazorChat.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class UpdateMemberRoleCommandHandler(
    IServerAuthorizationService authService,
    IServerRepository serverRepository,
    IHubContext<ServerHub, IServerHubClient> hubContext)
    : ICommandHandler<UpdateMemberRoleCommand, ChannelResult<bool>>
{
    public async ValueTask<ChannelResult<bool>> Handle(UpdateMemberRoleCommand request, CancellationToken ct)
    {
        var currentRole = await authService.GetUserRoleInServerAsync(request.ServerId, request.CurrentUserId, ct);
        if (currentRole != ServerRole.Owner)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.Forbidden, ErrorMessage: "Only the server owner can manage member roles.");
        }

        if (request.TargetUserId == request.CurrentUserId)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "You cannot change your own ownership status through this endpoint.");
        }

        if (request.NewRole == ServerRole.Owner)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.BadRequest, ErrorMessage: "Ownership transfer requires a specialized command workflow.");
        }

        var success = await serverRepository.UpdateMemberRoleAsync(request.ServerId, request.TargetUserId, request.NewRole, ct);
        
        if (!success)
        {
            return new ChannelResult<bool>(false, Error: ChannelError.NotFound, ErrorMessage: "The specified target user is not a member of this server.");
        }

        await serverRepository.SaveChangesAsync(ct);

        try
        {
            await hubContext.Clients
                .Group($"server_{request.ServerId}")
                .UserRoleUpdated(request.ServerId, request.TargetUserId, request.NewRole);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR live role push notification broadcast dropped gracefully: {ex.Message}");
        }

        return new ChannelResult<bool>(true, Data: true);
    }
}