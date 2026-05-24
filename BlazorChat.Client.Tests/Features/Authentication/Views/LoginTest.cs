using System.Threading.Tasks;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Authentication.ViewModels;
using BlazorChat.Client.Features.Authentication.Views;
using BlazorChat.Shared.DTO;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Authentication.Views;

[TestFixture]
public class LoginTest : BunitTestContext
{
    private IAuthApiService _mockApiService = null!;
    private ICustomStateUpdater _mockStateUpdater = null!;

    [SetUp]
    public void LoginSetUp()
    {
        _mockApiService = Substitute.For<IAuthApiService>();
        _mockStateUpdater = Substitute.For<ICustomStateUpdater>();

        var anonymousState = new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(
            new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity()));
        _mockStateUpdater.GetAuthenticationStateAsync().Returns(Task.FromResult(anonymousState));

        TestContext.Services.AddSingleton(_mockApiService);
        TestContext.Services.AddSingleton(_mockStateUpdater);
        TestContext.Services.AddTransient<LoginViewModel>();
    }

    [Test]
    public void Login_WithValidCredentials_NavigatesToChat()
    {
        // Arrange
        _mockApiService.LoginAsync(Arg.Any<LoginDto>())
            .Returns(new MeDto { Id = 1, Username = "TestUser", Email = "test@test.com" });

        var cut = TestContext.Render<Login>();

        // Act
        cut.Find("input[placeholder='Email or Username']").Change("test@test.com");
        cut.Find("input[placeholder='Password']").Change("Password123!");
        
        cut.Find("form").Submit();

        // Assert
        var navMan = TestContext.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        Assert.That(navMan.Uri, Does.EndWith("/chat"));
        _mockStateUpdater.Received(1).NotifyUserAuthenticated(Arg.Is<MeDto>(m => m.Username == "TestUser"));
    }

    [Test]
    public void Login_WithInvalidCredentials_DisplaysErrorMessage()
    {
        // Arrange
        _mockApiService.LoginAsync(Arg.Any<LoginDto>())
            .Returns((MeDto?)null);

        var cut = TestContext.Render<Login>();

        // Act
        cut.Find("input[placeholder='Email or Username']").Change("wrong@test.com");
        cut.Find("input[placeholder='Password']").Change("WrongPassword");
        
        cut.Find("form").Submit();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Invalid credentials"));
        Assert.That(cut.Markup, Does.Contain("Invalid credentials"));
    }
}
