using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Users.Queries;

public record GetUserStatusQuery(int CurrentUserId) : IQuery<ReceiveUserStatus?>;