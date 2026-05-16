using BlazorChat.Server.Application.Features.Channels.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class GetMyDmsQueryHandler(AppDbContext db) : IQueryHandler<GetMyDmsQuery, List<ChannelDto>>
{
    public async ValueTask<List<ChannelDto>> Handle(GetMyDmsQuery request, CancellationToken ct)
    {
        return await db.Channels
            .AsNoTracking()
            .Include(c => c.Members)
            .Where(c => c.Type == ChannelType.DirectMessage && c.Members.Any(m => m.Id == request.CurrentUserId))
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ChannelDto
            {
                Id = c.Id,
                Type =  c.Type,
                Members = c.Members.Select(m => new UserDto
                {
                    Id = m.Id,
                    Username = m.Username,
                    AvatarUrl = null
                }).ToList()
            })
            .ToListAsync(ct);
    }
}