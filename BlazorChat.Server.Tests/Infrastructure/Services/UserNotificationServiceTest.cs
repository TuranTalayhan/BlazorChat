using BlazorChat.Server.Hubs;
using BlazorChat.Server.Infrastructure.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class UserNotificationServiceTests
{
    private IHubContext<UserHub, IUserClient> _mockHubContext = null!;
    private IHubClients<IUserClient> _mockClients = null!;
    private IUserClient _mockUsersClientProxy = null!;
    
    private UserNotificationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHubContext = Substitute.For<IHubContext<UserHub, IUserClient>>();
        _mockClients = Substitute.For<IHubClients<IUserClient>>();
        _mockUsersClientProxy = Substitute.For<IUserClient>();

        _mockHubContext.Clients.Returns(_mockClients);

        _sut = new UserNotificationService(_mockHubContext);
    }

    [Test]
    public async Task SendUserStatusChangedAsync_ShouldDispatchPayload_ToExplicitListOfFriendUserIds()
    {
        var friendIds = new List<string> { "42", "101", "555" };
        var statusDto = new ReceiveUserStatusDto 
        { 
            Id = 12, 
            Status = UserStatus.Idle 
        };
        
        _mockClients.Users(friendIds).Returns(_mockUsersClientProxy);
        
        await _sut.SendUserStatusChangedAsync(friendIds, statusDto);
        
        await _mockUsersClientProxy.Received(1).UserStatusChanged(statusDto);
    }

    [Test]
    public async Task SendUserStatusChangedAsync_WhenFriendListIsEmpty_ShouldStillExecuteWithoutCrashing()
    {
        var emptyFriendIds = new List<string>().AsReadOnly();
        var statusDto = new ReceiveUserStatusDto { Id = 12, Status = UserStatus.Offline };

        _mockClients.Users(emptyFriendIds).Returns(_mockUsersClientProxy);
        
        Assert.DoesNotThrowAsync(async () => 
            await _sut.SendUserStatusChangedAsync(emptyFriendIds, statusDto)
        );

        await _mockUsersClientProxy.Received(1).UserStatusChanged(statusDto);
    }
}