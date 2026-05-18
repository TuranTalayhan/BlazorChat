namespace BlazorChat.Shared.DTO;

public record SidebarFriendSummaryDto(
    int FriendId,
    string Username,
    string? AvatarUrl,
    UserStatus Status,
    int ChannelId,
    bool HasUnreadMessages
);