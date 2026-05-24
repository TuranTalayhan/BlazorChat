using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Dialogs.JoinServer;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using NUnit.Framework;

using BlazorChat.Client.Features.Servers.ViewModels;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.JoinServer;

[TestFixture]
public class JoinServerModalTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        TestContext.Services.AddSingleton(_mockApiService);
        TestContext.Services.AddSingleton(Substitute.For<IServerApiService>());
        TestContext.Services.AddSingleton(Substitute.For<ISnackbar>());
        TestContext.Services.AddMudServices();
    }

    [Test]
    public async Task Submit_WhenValid_ClosesDialogWithResult()
    {
        var provider = TestContext.Render<MudDialogProvider>();
        var dialogService = TestContext.Services.GetRequiredService<IDialogService>();

        _mockApiService.JoinServerWithCodeAsync("ABCDEF")
            .Returns(new BlazorChat.Client.Core.ApiResponse<ServerDto> { IsSuccess = true, Data = new ServerDto { Id = 10, Name = "Joined Server" } });

        IDialogReference? dialogReference = null;
        
        await provider.InvokeAsync(async () =>
        {
            dialogReference = await dialogService.ShowAsync<JoinServerModal>("Join");
        });

        Assert.That(provider.Markup, Does.Contain("Join Server"));

        var input = provider.Find("input");
        await provider.InvokeAsync(() => input.Change("ABCDEF"));

        var submitBtn = provider.FindAll("button").FirstOrDefault(b => b.InnerHtml.Contains("Join Server"));
        Assert.That(submitBtn, Is.Not.Null);

        await provider.InvokeAsync(() => submitBtn!.Click());

        var result = await dialogReference!.Result;
        Assert.That(result!.Canceled, Is.False);
        var dto = result.Data as ServerDto;
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(10));
    }
}
