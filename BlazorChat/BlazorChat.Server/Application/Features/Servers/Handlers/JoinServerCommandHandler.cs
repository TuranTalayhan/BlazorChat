using BlazorChat.Server.Application.Common;
using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class JoinServerCommandHandler(IServerRepository serverRepository) 
    : ICommandHandler<JoinServerCommand, Result<ServerDto>>
{
    public async ValueTask<Result<ServerDto>> Handle(JoinServerCommand request, CancellationToken ct)
    {
        var invite = await serverRepository.GetInviteWithServerByCodeAsync(request.Code, ct);

        if (invite == null || invite.IsExpired || invite.IsLimitReached)
        {
            return Result<ServerDto>.Failure("This invitation code is invalid, expired, or max limit reached.");
        }
        
        var alreadyMember = await serverRepository.IsMemberAsync(invite.ServerId, request.UserId, ct);
        if (alreadyMember)
        {
            return Result<ServerDto>.Failure("You are already a member of this server.");
        }

        var membership = new ServerMembership
        {
            ServerId = invite.ServerId,
            UserId = request.UserId,
            Role = ServerRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        invite.Uses++;
        await serverRepository.AddMembershipAsync(membership, ct);
        await serverRepository.SaveChangesAsync(ct);

        return Result<ServerDto>.Success(new ServerDto 
        { 
            Id = invite.Server.Id, 
            Name = invite.Server.Name 
        });
    }
}