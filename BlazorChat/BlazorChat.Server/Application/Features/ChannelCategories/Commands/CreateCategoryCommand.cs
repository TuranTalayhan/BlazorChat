using BlazorChat.Server.Application.Features.Servers;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Commands;

public record CreateCategoryCommand(
    int CurrentUserId, 
    int ServerId, 
    string Name
) : ICommand<ServerResult<CategoryDto>>;