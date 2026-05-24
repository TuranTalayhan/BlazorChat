using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class DirectMessagesControllerIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [SetUp]
    public async Task SetUp()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);


        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        

        db.Channels.RemoveRange(db.Channels);
        db.Friendships.RemoveRange(db.Friendships);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        var testUser = new User { Id = 1, Username = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        var friendUser = new User { Id = 2, Username = "FriendUser", Email = "friend@example.com", PasswordHash = "hash" };
        
        db.Users.Add(testUser);
        db.Users.Add(friendUser);
        db.Friendships.Add(new Friendship { RequesterId = 1, ReceiverId = 2, Status = FriendshipStatus.Accepted });
        
        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task OpenDirectMessage_WhenValid_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/dms", 2);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var channelId = await response.Content.ReadFromJsonAsync<int>();
        channelId.Should().BeGreaterThan(0);
        
        // Calling again should return 200 OK since it's already created
        var response2 = await _client.PostAsJsonAsync("/api/dms", 2);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetMyDirectMessages_ReturnsListOfDms()
    {
        await _client.PostAsJsonAsync("/api/dms", 2);

        var response = await _client.GetAsync("/api/dms");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dms = await response.Content.ReadFromJsonAsync<List<ChannelDto>>();
        dms.Should().NotBeNull();
        dms!.Count.Should().BeGreaterThan(0);
        dms[0].Type.Should().Be(ChannelType.DirectMessage);
    }
}
