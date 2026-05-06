using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetMyServersQueryHandler(AppDbContext db) : IQueryHandler<GetMyServersQuery, List<ServerDto>>
{
    public async ValueTask<List<ServerDto>> Handle(GetMyServersQuery request, CancellationToken ct)
    {
        return await db.ServerMemberships
            .AsNoTracking()
            .Where(sm => sm.UserId == request.CurrentUserId)
            .Include(sm => sm.Server)
            .Select(sm => new ServerDto
            {
                Id = sm.Server.Id,
                Name = sm.Server.Name,
                OwnerId = sm.Server.OwnerId
            })
            .ToListAsync(ct);
    }
}