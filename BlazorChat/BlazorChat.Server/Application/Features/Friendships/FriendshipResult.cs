namespace BlazorChat.Server.Application.Features.Friendships;

public record FriendshipResult(bool IsSuccess, FriendshipError Error = FriendshipError.None, string? ErrorMessage = null);