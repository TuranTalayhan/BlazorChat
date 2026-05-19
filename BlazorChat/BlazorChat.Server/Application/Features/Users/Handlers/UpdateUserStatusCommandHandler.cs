using BlazorChat.Server.Application.Features.Users.Commands;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class UpdateUserStatusCommandHandler(IUserRepository userRepository, IUserNotificationService notifications) 
    : ICommandHandler<UpdateUserStatusCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateUserStatusCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.CurrentUserId, ct);
        if (user == null) return false;

        var friendIds = await userRepository.GetAcceptedFriendIdsAsync(request.CurrentUserId, ct);

        user.UpdateStatus(request.Dto.Status);
        await userRepository.SaveChangesAsync(ct);

        var statusMsg = new ReceiveUserStatusDto
        {
            Id = request.CurrentUserId,
            Status = request.Dto.Status
        };
        await notifications.SendUserStatusChangedAsync(friendIds, statusMsg);

        return true;
    }
}