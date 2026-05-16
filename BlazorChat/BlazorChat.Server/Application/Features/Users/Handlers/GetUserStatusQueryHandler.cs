using BlazorChat.Server.Application.Features.Users.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class GetUserStatusQueryHandler(AppDbContext db) : IQueryHandler<GetUserStatusQuery, ReceiveUserStatusDto?>
{
    public async ValueTask<ReceiveUserStatusDto?> Handle(GetUserStatusQuery request, CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, ct);

        if (user == null) return null;

        return new ReceiveUserStatusDto
        {
            Id = user.Id,
            Status = user.Status
        };
    }
}