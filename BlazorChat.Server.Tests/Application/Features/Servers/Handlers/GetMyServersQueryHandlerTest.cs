using BlazorChat.Server.Application.Features.Servers.Handlers;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlazorChat.Tests.Server.Application.Features.Servers.Handlers;

public class GetMyServersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasServers_ShouldReturnOnlyTheirServers()
    {
        // --- 1. ARRANGE (Setup the database) ---
        var currentUserId = 1;
        var otherUserId = 2;
        
        await using var db = CreateTestDatabase(); 
        
        // Seed the database with fake data
        var myServer = new ChatServer { Id = 10, Name = "My Cool Server", OwnerId = currentUserId };
        var otherServer = new ChatServer { Id = 20, Name = "Someone Else's Server", OwnerId = otherUserId };
        
        db.Servers.AddRange(myServer, otherServer);
        
        // Add memberships
        db.ServerMemberships.Add(new ServerMembership { ServerId = myServer.Id, UserId = currentUserId, Role = ServerRole.Owner });
        db.ServerMemberships.Add(new ServerMembership { ServerId = otherServer.Id, UserId = otherUserId, Role = ServerRole.Owner });
        
        await db.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken); // Save the seed data to memory

        var handler = new GetMyServersQueryHandler(db);
        var query = new GetMyServersQuery(currentUserId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("My Cool Server");
    }

    private static AppDbContext CreateTestDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}