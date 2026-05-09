using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Servers.Queries;

public record GetServerCategoriesQuery(int CurrentUserId, int ServerId) 
    : IQuery<ServerResult<List<CategoryDto>>>;