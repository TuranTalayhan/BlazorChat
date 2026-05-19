using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Hubs;
using BlazorChat.Server.Infrastructure.Services;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace BlazorChat.Tests.Server.Infrastructure.Services;

[TestFixture]
public class ChatNotificationServiceTests
{
    private IHubContext<ChatHub, IChatClient> _mockHubContext = null!;
    private IHubClients<IChatClient> _mockClients = null!;
    private IChatClient _mockGroupClient = null!;
    private IChatClient _mockUserClient = null!;
    private IChatPresenceTracker _mockPresenceTracker = null!;
    
    private ChatNotificationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockHubContext = Substitute.For<IHubContext<ChatHub, IChatClient>>();
        _mockClients = Substitute.For<IHubClients<IChatClient>>();
        _mockGroupClient = Substitute.For<IChatClient>();
        _mockUserClient = Substitute.For<IChatClient>();
        _mockPresenceTracker = Substitute.For<IChatPresenceTracker>();

        _mockHubContext.Clients.Returns(_mockClients);
        
        _sut = new ChatNotificationService(_mockHubContext, _mockPresenceTracker);
    }

    [Test]
    public async Task SendMessage_ShouldAlwaysBroadcastToChannelGroup()
    {
        var channelId = 5;
        var message = new MessageDto { Id = 1, Content = "Hello!" };
        _mockClients.Group($"channel:{channelId}").Returns(_mockGroupClient);

        await _sut.SendMessageToChannelAsync(channelId, recipientUserId: 0, message);

        await _mockGroupClient.Received(1).ReceiveMessage(message);
    }

    [Test]
    public async Task SendMessage_WhenRecipientIsActiveInRoom_ShouldNotSendDuplicateDirectMessage()
    {
        var channelId = 10;
        var recipientId = 99;
        var message = new MessageDto { Id = 2, Content = "Ping" };

        _mockClients.Group($"channel:{channelId}").Returns(_mockGroupClient);
        _mockPresenceTracker.IsUserActiveInChannel(channelId, recipientId).Returns(true);

        await _sut.SendMessageToChannelAsync(channelId, recipientId, message);

        await _mockGroupClient.Received(1).ReceiveMessage(message);
        _mockClients.DidNotReceive().User(Arg.Any<string>());
    }

    [Test]
    public async Task SendMessage_WhenRecipientIsOfflineOrInAnotherRoom_ShouldSendDirectNotificationAlert()
    {
        var channelId = 10;
        var recipientId = 99;
        var message = new MessageDto { Id = 3, Content = "Unread message alert" };

        _mockClients.Group($"channel:{channelId}").Returns(_mockGroupClient);
        _mockClients.User(recipientId.ToString()).Returns(_mockUserClient);
        _mockPresenceTracker.IsUserActiveInChannel(channelId, recipientId).Returns(false); // User is elsewhere!

        await _sut.SendMessageToChannelAsync(channelId, recipientId, message);

        await _mockGroupClient.Received(1).ReceiveMessage(message);
        await _mockUserClient.Received(1).ReceiveMessage(message);
    }
}