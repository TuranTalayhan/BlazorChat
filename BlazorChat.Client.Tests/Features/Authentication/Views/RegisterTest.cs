using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Authentication.Views;
using BlazorChat.Shared.DTO;
using BlazorChat.Tests.Client.Mocks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Authentication.Views;

[TestFixture]
public class RegisterTest : BunitTestContext
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void RegisterSetUp()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        TestContext.Services.AddSingleton(_httpClient);
        TestContext.Services.AddSingleton<ChatAuthStateProvider>();
    }

    [TearDown]
    public void RegisterTearDown()
    {
        _httpClient?.Dispose();
        _mockHttp?.Dispose();
    }

    [Test]
    public void Register_WithValidData_NavigatesToChat()
    {
        // Arrange
        var responseDto = new MeDto { Id = 1, Username = "TestUser", Email = "test@test.com" };
        _mockHttp.ExpectPost("api/auth/register", responseDto);

        var cut = TestContext.Render<Register>();

        // Act
        cut.Find("input[placeholder='Username']").Change("TestUser");
        cut.Find("input[placeholder='Email']").Change("test@test.com");
        cut.Find("input[placeholder='Password']").Change("Password123!");
        cut.Find("input[placeholder='Confirm Password']").Change("Password123!");
        
        cut.Find("form").Submit();

        // Assert
        
        var navMan = TestContext.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        Assert.That(navMan.Uri, Does.EndWith("/chat"));
    }

    [Test]
    public void Register_WithApiError_DisplaysErrorMessage()
    {
        // Arrange
        _mockHttp.ExpectPostError("api/auth/register", HttpStatusCode.BadRequest, new { Message = "Username already exists" });

        var cut = TestContext.Render<Register>();

        // Act
        cut.Find("input[placeholder='Username']").Change("ExistingUser");
        cut.Find("input[placeholder='Email']").Change("test@test.com");
        cut.Find("input[placeholder='Password']").Change("Password123!");
        cut.Find("input[placeholder='Confirm Password']").Change("Password123!");
        
        cut.Find("form").Submit();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Username already exists"));
        Assert.That(cut.Markup, Does.Contain("Username already exists"));
    }
}
