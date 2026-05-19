using BlazorChat.Server.Application.Features.Users.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class GetUserStatusQueryHandler(IUserRepository userRepository) 
    : IQueryHandler<GetUserStatusQuery, ReceiveUserStatusDto?>
{
    public async ValueTask<ReceiveUserStatusDto?> Handle(GetUserStatusQuery request, CancellationToken ct)
    {
        return await userRepository.GetUserStatusAsync(request.CurrentUserId, ct);
    }
}