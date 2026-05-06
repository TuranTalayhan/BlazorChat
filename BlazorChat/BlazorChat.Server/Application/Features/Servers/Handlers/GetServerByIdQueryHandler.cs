using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerByIdQueryHandler(AppDbContext db) : IQueryHandler<GetServerByIdQuery, ServerResult<ServerDto>>
{
    public async ValueTask<ServerResult<ServerDto>> Handle(GetServerByIdQuery request, CancellationToken ct)
    {
        var membership = await db.ServerMemberships
            .AsNoTracking()
            .Include(sm => sm.Server)
            .FirstOrDefaultAsync(sm => sm.ServerId == request.ServerId && sm.UserId == request.CurrentUserId, ct);

        if (membership == null)
        {
            var serverExists = await db.Servers.AnyAsync(s => s.Id == request.ServerId, ct);
            return new ServerResult<ServerDto>(false, Error: serverExists ? ServerError.Forbidden : ServerError.NotFound);
        }
        
        var dto = new ServerDto
        {
            Id = membership.Server.Id,
            Name = membership.Server.Name,
            OwnerId = membership.Server.OwnerId
        };

        return new ServerResult<ServerDto>(true, Data: dto);
    }
}