using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Commands;

public record DeleteCategoryCommand(int UserId, int CategoryId) : ICommand<CategoryResult>;