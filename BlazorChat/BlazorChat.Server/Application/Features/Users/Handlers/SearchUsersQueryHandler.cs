using BlazorChat.Server.Application.Features.Users.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Handlers;

public class SearchUsersQueryHandler(IUserRepository userRepository) : IQueryHandler<SearchUsersQuery, List<string>>
{
    public async ValueTask<List<string>> Handle(SearchUsersQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
        {
            return [];
        }

        return await userRepository.SearchUsernamesAsync(
            request.CurrentUserId, 
            request.SearchTerm, 
            limit: 5, 
            ct
        );
    }
}