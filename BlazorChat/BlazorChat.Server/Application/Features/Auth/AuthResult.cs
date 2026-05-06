using BlazorChat.Shared.DTO;

namespace BlazorChat.Server.Application.Features.Auth;

public record AuthResult(bool IsSuccess, string? ErrorMessage, MeDto? User = null);