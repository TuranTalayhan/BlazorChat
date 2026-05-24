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
public class UsersControllerIntegrationTests
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
        
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        var testUser = new User { Id = 1, Username = "TestUser", Email = "test@example.com", PasswordHash = "hash" };
        var otherUser = new User { Id = 2, Username = "SearchUser", Email = "search@example.com", PasswordHash = "hash" };
        
        db.Users.Add(testUser);
        db.Users.Add(otherUser);
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
    public async Task Search_ReturnsMatchingUsers()
    {
        var response = await _client.GetAsync("/api/users/search?q=SearchUser");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<string>>();
        users.Should().NotBeNull();
        users!.Count.Should().Be(1);
        users[0].Should().Be("SearchUser");
    }

    [Test]
    public async Task GetStatus_ReturnsUserStatus()
    {
        var response = await _client.GetAsync("/api/users/me/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusDto = await response.Content.ReadFromJsonAsync<ReceiveUserStatusDto>();
        statusDto.Should().NotBeNull();
    }
    
    [Test]
    public async Task UpdateStatus_ReturnsOk()
    {
        var dto = new UpdateStatusDto { Status = UserStatus.DoNotDisturb };
        var response = await _client.PatchAsJsonAsync("/api/users/me/status", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var verifyResponse = await _client.GetAsync("/api/users/me/status");
        var statusDto = await verifyResponse.Content.ReadFromJsonAsync<ReceiveUserStatusDto>();
        statusDto!.Status.Should().Be(UserStatus.DoNotDisturb);
    }
}
