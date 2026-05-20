using BlazorChat.Server.Hubs;
using BlazorChat.Server.Infrastructure.Services;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class FriendNotificationServiceTests
{
    private IHubContext<FriendHub, IFriendClient> _mockHubContext = null!;
    private IHubClients<IFriendClient> _mockClients = null!;
    private IFriendClient _mockTargetUserClient = null!;
    
    private FriendNotificationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHubContext = Substitute.For<IHubContext<FriendHub, IFriendClient>>();
        _mockClients = Substitute.For<IHubClients<IFriendClient>>();
        _mockTargetUserClient = Substitute.For<IFriendClient>();

        _mockHubContext.Clients.Returns(_mockClients);
        
        _sut = new FriendNotificationService(_mockHubContext);
    }

    [Test]
    public async Task SendFriendRequestAsync_ShouldDispatchPayload_ToTargetUserConnectionString()
    {
        var targetUserId = "42";
        var payloadDto = new PendingFriendshipDto 
        { 
            RequesterId = 10, 
            RequesterUsername = "Alice" 
        };

        _mockClients.User(targetUserId).Returns(_mockTargetUserClient);
        
        await _sut.SendFriendRequestAsync(targetUserId, payloadDto);
        
        await _mockTargetUserClient.Received(1).ReceiveFriendRequest(payloadDto);
    }

    [Test]
    public async Task SendNewFriendAddedAsync_ShouldDispatchPayload_ToTargetUserConnectionString()
    {
        var targetUserId = "99";
        var payloadDto = new FriendshipDto 
        { 
            FriendId = 5, 
            Username = "Bob", 
            Status = UserStatus.Online 
        };

        _mockClients.User(targetUserId).Returns(_mockTargetUserClient);

        await _sut.SendNewFriendAddedAsync(targetUserId, payloadDto);
        
        await _mockTargetUserClient.Received(1).ReceiveNewFriend(payloadDto);
    }
}