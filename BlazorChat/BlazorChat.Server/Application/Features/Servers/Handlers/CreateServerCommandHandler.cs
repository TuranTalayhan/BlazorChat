using BlazorChat.Server.Application.Features.Servers.Commands;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class CreateServerCommandHandler(AppDbContext db) : ICommandHandler<CreateServerCommand, ServerDto>
{
    public async ValueTask<ServerDto> Handle(CreateServerCommand request, CancellationToken ct)
    {
        // 1. Create the server
        var server = new ChatServer
        {
            Name = request.Dto.Name,
            OwnerId = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };

        db.Servers.Add(server);
        await db.SaveChangesAsync(ct);

        // 2. Scaffold default channel and assign ownership
        db.Channels.Add(new Channel { Name = "general", ServerId = server.Id });
        db.ServerMemberships.Add(new ServerMembership 
        { 
            ServerId = server.Id, 
            UserId = request.CurrentUserId, 
            Role = ServerRole.Owner 
        });
        
        await db.SaveChangesAsync(ct);

        return new ServerDto 
        { 
            Id = server.Id, 
            Name = server.Name, 
            OwnerId = server.OwnerId 
        };
    }
}