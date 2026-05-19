using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class CreateServerCommandHandler(IServerRepository serverRepository) 
    : ICommandHandler<CreateServerCommand, ServerDto>
{
    public async ValueTask<ServerDto> Handle(CreateServerCommand request, CancellationToken ct)
    {
        var server = ChatServer.CreateWithDefaults(request.Dto.Name, request.CurrentUserId);

        await serverRepository.AddAsync(server, ct);
        await serverRepository.SaveChangesAsync(ct);

        return new ServerDto 
        { 
            Id = server.Id, 
            Name = server.Name, 
            OwnerId = server.OwnerId 
        };
    }
}