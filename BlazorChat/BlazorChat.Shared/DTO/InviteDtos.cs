using System.ComponentModel.DataAnnotations;

namespace BlazorChat.Shared.DTO;

public record CreateInviteDto
{
    [Range(1, 168, ErrorMessage = "Expiration must be between 1 and 168 hours.")]
    public int ExpiresInHours { get; set; } = 24;

    [Range(1, 100, ErrorMessage = "Max uses must be between 1 and 100.")]
    public int? MaxUses { get; set; }
}

public record InviteResponseDto
{
    public string Code { get; init; } = string.Empty;
    public string CreatedByUsername { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public int Uses { get; init; }
    public int? MaxUses { get; init; }
}