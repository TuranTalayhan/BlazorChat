using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class ServerMemberControllerIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [SetUp]
    public void SetUp()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
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
    public async Task GetServerMembers_WhenValid_ReturnsMembers()
    {
        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Members Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();

        var response = await _client.GetAsync($"/api/servers/{server!.Id}/members");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        members.Should().NotBeNull();
        members!.Count.Should().BeGreaterThan(0);
    }
}
