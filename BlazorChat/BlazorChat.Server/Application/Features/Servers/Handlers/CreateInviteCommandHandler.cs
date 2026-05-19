using BlazorChat.Server.Application.Common;
using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class CreateInviteCommandHandler(IServerRepository serverRepository) 
    : ICommandHandler<CreateInviteCommand, Result<InviteResponseDto>>
{
    public async ValueTask<Result<InviteResponseDto>> Handle(CreateInviteCommand request, CancellationToken ct)
    {
        var role = await serverRepository.GetUserRoleInServerAsync(request.ServerId, request.UserId, ct);
        if (role != ServerRole.Admin && role != ServerRole.Owner)
        {
            return Result<InviteResponseDto>.Failure("You do not have permission to create an invitation link.", ErrorType.Forbidden);
        }

        var invite = ServerInvite.Create(request.ServerId, request.UserId, request.Dto.ExpiresInHours, request.Dto.MaxUses);
        
        await serverRepository.AddInviteAsync(invite, ct);
        await serverRepository.SaveChangesAsync(ct);

        return Result<InviteResponseDto>.Success(new InviteResponseDto
        {
            Code = invite.Code,
            ExpiresAt = invite.ExpiresAt,
            MaxUses = invite.MaxUses,
            Uses = 0
        });
    }
}