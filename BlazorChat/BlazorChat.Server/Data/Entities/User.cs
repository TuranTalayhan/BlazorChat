using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Data.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } 

    // Relationships
    public ICollection<Friendship> SentRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedRequests { get; set; } = new List<Friendship>();
}