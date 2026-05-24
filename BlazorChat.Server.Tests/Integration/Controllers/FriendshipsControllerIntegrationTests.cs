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
public class FriendshipsControllerIntegrationTests
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
        

        db.Friendships.RemoveRange(db.Friendships);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        var testUser = new User { Id = 1, Username = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        var friendUser = new User { Id = 2, Username = "FriendUser", Email = "friend@example.com", PasswordHash = "hash" };
        
        db.Users.Add(testUser);
        db.Users.Add(friendUser);
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
    public async Task SendRequest_WhenValid_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/friendships", "FriendUser");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task SendRequest_WhenUserNotFound_ReturnsNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/friendships", "UnknownUser");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetFriends_ReturnsAcceptedFriendships()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Friendships.Add(new Friendship { RequesterId = 1, ReceiverId = 2, Status = FriendshipStatus.Accepted });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/friendships");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var friends = await response.Content.ReadFromJsonAsync<List<FriendshipDto>>();
        friends.Should().NotBeNull();
        friends!.Count.Should().Be(1);
        friends[0].Username.Should().Be("FriendUser");
    }

    [Test]
    public async Task GetPendingRequests_ReturnsPendingFriendships()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Friendships.Add(new Friendship { RequesterId = 2, ReceiverId = 1, Status = FriendshipStatus.Pending });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/friendships/pending");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pending = await response.Content.ReadFromJsonAsync<List<PendingFriendshipDto>>();
        pending.Should().NotBeNull();
        pending!.Count.Should().Be(1);
    }
    
    [Test]
    public async Task RespondToRequest_WhenValid_ReturnsOk()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Friendships.Add(new Friendship { RequesterId = 2, ReceiverId = 1, Status = FriendshipStatus.Pending });
            await db.SaveChangesAsync();
        }

        var response = await _client.PatchAsJsonAsync("/api/friendships/2", true);
        response.StatusCode.Should().Be(HttpStatusCode.OK);


        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var friendship = await db.Friendships.FindAsync(2, 1);
            friendship!.Status.Should().Be(FriendshipStatus.Accepted);
        }
    }
}
