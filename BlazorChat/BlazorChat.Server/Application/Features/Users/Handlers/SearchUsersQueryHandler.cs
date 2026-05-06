using BlazorChat.Server.Application.Features.Users.Queries;
using BlazorChat.Server.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class SearchUsersQueryHandler(AppDbContext db) : IQueryHandler<SearchUsersQuery, List<string>>
{
    public async ValueTask<List<string>> Handle(SearchUsersQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
            return [];

        return await db.Users
            .AsNoTracking()
            .Where(u => u.Id != request.CurrentUserId && u.Username.ToLower().Contains(request.SearchTerm.ToLower()))
            .Take(5)
            .Select(u => u.Username)
            .ToListAsync(ct);
    }
}