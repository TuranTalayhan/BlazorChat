using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Domain.Entities;

public class User
{
    public int Id { get; init; }

    [Required, MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(32)]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Offline;

    [MaxLength(512)]
    public string? AvatarUrl { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Friendship> SentRequests { get; init; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedRequests { get; init; } = new List<Friendship>();
    public ICollection<ServerMembership> ServerMemberships { get; init; } = new List<ServerMembership>();
    
    public static User Create(string username, string email, string password, Func<User, string, string> hashStrategy)
    {
        var user = new User
        {
            Username = username.Trim(),
            Email = email.Trim().ToLower(),
            Status = UserStatus.Online,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = hashStrategy(user, password);
        return user;
    }
}