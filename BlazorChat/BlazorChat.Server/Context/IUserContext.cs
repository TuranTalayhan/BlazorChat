using System.Security.Claims;

namespace BlazorChat.Server.Context;

public interface IUserContext
{
    int UserId { get; }
}

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public int UserId => int.TryParse(
        accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, 
        out var id) ? id : 0;
}