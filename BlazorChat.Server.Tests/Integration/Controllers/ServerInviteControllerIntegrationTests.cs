using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Shared.DTO;
using BlazorChat.Shared.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class ServerInviteControllerIntegrationTests
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
    public async Task CreateInvite_WhenValid_ReturnsInvite()
    {

        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Invite Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();


        var inviteDto = new CreateInviteDto { MaxUses = 5, ExpiresInHours = 24 };
        var response = await _client.PostAsJsonAsync($"/api/servers/{server!.Id}/invites", inviteDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invite = await response.Content.ReadFromJsonAsync<InviteResponseDto>();
        invite.Should().NotBeNull();
        invite!.Code.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetUserRole_WhenValid_ReturnsRole()
    {
        var createServerResponse = await _client.PostAsJsonAsync("/api/servers", new CreateServerDto { Name = "Role Server" });
        var server = await createServerResponse.Content.ReadFromJsonAsync<ServerDto>();

        var response = await _client.GetAsync($"/api/servers/{server!.Id}/role");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<ServerRole>();
        role.Should().Be(ServerRole.Owner); // The creator is the owner
    }
}
