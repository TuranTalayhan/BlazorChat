using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.ChannelCategories.Commands;

public record UpdateCategoryCommand(
    int UserId, 
    int CategoryId, 
    UpdateCategoryDto Dto) : ICommand<CategoryResult>;