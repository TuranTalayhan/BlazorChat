using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Servers.Handlers;

public class GetServerChannelsQueryHandler(AppDbContext db) : IQueryHandler<GetServerChannelsQuery, ServerResult<List<ChannelDto>>>
{
    public async ValueTask<ServerResult<List<ChannelDto>>> Handle(GetServerChannelsQuery request, CancellationToken ct)
    {
        var isMember = await db.ServerMemberships
            .AnyAsync(sm => sm.ServerId == request.ServerId && sm.UserId == request.CurrentUserId, ct);
            
        if (!isMember) 
            return new ServerResult<List<ChannelDto>>(false, Error: ServerError.Forbidden);
        
        var channels = await db.Channels
            .AsNoTracking()
            .Where(c => c.ServerId == request.ServerId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CreatedAt)
            .Select(c => new ChannelDto 
            { 
                Id = c.Id, 
                Name = c.Name, 
                Type = c.Type,
                ServerId = c.ServerId, 
                SortOrder = c.SortOrder,
                Category = c.Category != null ? new CategoryDto
                {
                    Id = c.Category.Id,
                    Name = c.Category.Name,
                    SortOrder = c.Category.SortOrder
                } : null
            })
            .ToListAsync(ct);

        return new ServerResult<List<ChannelDto>>(true, Data: channels);
    }
}