using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Dialogs.CreateChannel;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.CreateChannel;

[TestFixture]
public class CreateChannelDialogTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        TestContext.Services.AddSingleton(new CreateChannelViewModel(_mockApiService));
        
        TestContext.Services.AddMudServices();
    }

    [Test]
    public async Task Dialog_CanBeRenderedAndSubmitted()
    {
        // Add the MudDialogProvider so DialogService works
        var provider = TestContext.Render<MudDialogProvider>();

        var dialogService = TestContext.Services.GetRequiredService<IDialogService>();

        _mockApiService.CreateChannelAsync(1, Arg.Any<CreateServerChannelDto>())
            .Returns(new ChannelDto { Id = 5, Name = "new-chan" });

        // Show dialog
        var parameters = new DialogParameters { { "ServerId", 1 } };
        IDialogReference? dialogReference = null;
        
        await provider.InvokeAsync(async () =>
        {
            dialogReference = await dialogService.ShowAsync<CreateChannelDialog>("Create", parameters);
        });

        // The dialog is rendered inside the provider's markup
        Assert.That(provider.Markup, Does.Contain("Create Text Channel"));
        Assert.That(provider.Markup, Does.Contain("Channel Name"));

        var input = provider.Find("input");
        await provider.InvokeAsync(() => input.Input("new-chan"));

        var submitBtn = provider.FindAll("button").FirstOrDefault(b => b.InnerHtml.Contains("Create Channel"));
        Assert.That(submitBtn, Is.Not.Null);

        await provider.InvokeAsync(() => submitBtn!.Click());

        var result = await dialogReference!.Result;
        Assert.That(result!.Canceled, Is.False);
        var dto = result.Data as ChannelDto;
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(5));
    }
}
