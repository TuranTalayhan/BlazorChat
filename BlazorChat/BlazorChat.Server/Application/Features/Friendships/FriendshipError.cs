namespace BlazorChat.Server.Application.Features.Friendships;

public enum FriendshipError
{
    None, 
    NotFound, 
    Conflict, 
    BadRequest
}