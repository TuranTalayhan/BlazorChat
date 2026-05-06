using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Queries;

public record SearchUsersQuery(int CurrentUserId, string SearchTerm) : IQuery<List<string>>;