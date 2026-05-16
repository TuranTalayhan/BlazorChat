using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public class UserDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
}