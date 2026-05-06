using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Application.Features.Messages.Handlers;
using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlazorChat.Tests.Server.Application.Features.Messages.Handlers;


public class SendMessageCommandHandlerTest
{
    private readonly AppDbContext _db;
    private readonly Mock<IChannelAuthorizationService> _authMock;
    private readonly Mock<IChatNotificationService> _notifyMock;
    private readonly SendMessageCommandHandler _handler;

    public SendMessageCommandHandlerTest()
    {
        // 1. Setup a fresh In-Memory Database for every test
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        // 2. Setup our Mocks (Fakes)
        _authMock = new Mock<IChannelAuthorizationService>();
        _notifyMock = new Mock<IChatNotificationService>();

        // 3. Initialize the handler with the fake dependencies
        _handler = new SendMessageCommandHandler(_db, _authMock.Object, _notifyMock.Object);
    }

    [Fact] // This tells xUnit this is a test method
    public async Task Handle_WhenUserIsAuthorized_ShouldSaveMessageAndNotify()
    {
        // --- ARRANGE (Setup the test data) ---
        var currentUserId = 1;
        var channelId = 99;
        
        // Seed the fake database with a user
        _db.Users.Add(new User { Id = currentUserId, Username = "TestUser" });
        await _db.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

        // Tell our fake Authorization service to return 'true'
        _authMock.Setup(a => a.CanAccessChannelAsync(currentUserId, channelId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

        var command = new SendMessageCommand(currentUserId, new SendMessageDto 
        { 
            ChannelId = channelId, 
            Content = "Hello World!" 
        });

        // --- ACT (Run the handler) ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT (Verify the results) ---
        // 1. Verify the result object
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Content.Should().Be("Hello World!");

        // 2. Verify it was actually saved to the database
        var savedMessage = await _db.Messages.FirstOrDefaultAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
        savedMessage.Should().NotBeNull();
        savedMessage!.ChannelId.Should().Be(channelId);

        // 3. Verify that SignalR was told to broadcast the message!
        _notifyMock.Verify(n => n.SendMessageToChannelAsync(
            channelId, 
            It.Is<MessageDto>(m => m.Content == "Hello World!")
        ), Times.Once);
    }
}