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
        
    }
}