using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public enum UserStatus
{
    Offline,
    Online,
    Idle,
    DoNotDisturb
}

public class UserDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
}