using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BlazorChat.Tests.Server.Integration.Controllers;

[TestFixture]
public class AuthControllerIntegrationTests
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

        // Clear users for clean auth testing
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
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
    public async Task Register_WhenValid_ReturnsUserAndSetsCookie()
    {
        var registerDto = new CreateUserDto 
        { 
            Username = "new_auth_user", 
            Email = "new_auth@example.com", 
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var user = await response.Content.ReadFromJsonAsync<MeDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("new_auth_user");
    }

    [Test]
    public async Task Login_WhenValid_ReturnsUser()
    {

        var registerDto = new CreateUserDto { Username = "login_user", Email = "login@example.com", Password = "Password123!", ConfirmPassword = "Password123!" };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);


        var loginDto = new LoginDto { Email = "login@example.com", Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<MeDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("login_user");
    }

    [Test]
    public async Task Me_WhenAuthenticated_ReturnsMeDto()
    {
        // TestAuthHandler intercepts this because of our DefaultScheme setup
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<MeDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("TestUser");
    }
}
