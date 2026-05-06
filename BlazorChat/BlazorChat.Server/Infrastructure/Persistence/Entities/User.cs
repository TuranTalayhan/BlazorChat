using System.ComponentModel.DataAnnotations;
using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Infrastructure.Persistence.Entities;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Offline;

    [MaxLength(512)]
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public ICollection<Friendship> SentRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedRequests { get; set; } = new List<Friendship>();
    public ICollection<ServerMembership> ServerMemberships { get; set; } = new List<ServerMembership>();
}