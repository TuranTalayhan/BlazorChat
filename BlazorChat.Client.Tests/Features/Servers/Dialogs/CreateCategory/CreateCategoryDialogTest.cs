using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Client.Features.Servers.Dialogs.CreateCategory;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Shared.DTO;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using NUnit.Framework;

namespace BlazorChat.Tests.Client.Features.Servers.Dialogs.CreateCategory;

[TestFixture]
public class CreateCategoryDialogTest : BunitTestContext
{
    private IChannelsApiService _mockApiService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApiService = Substitute.For<IChannelsApiService>();
        TestContext.Services.AddSingleton(new CreateCategoryViewModel(_mockApiService));
        
        TestContext.Services.AddMudServices();
    }

    [Test]
    public async Task Dialog_CanBeRenderedAndSubmitted()
    {
        var provider = TestContext.Render<MudDialogProvider>();
        var dialogService = TestContext.Services.GetRequiredService<IDialogService>();

        _mockApiService.CreateCategoryAsync(1, Arg.Any<CreateCategoryDto>())
            .Returns(new BlazorChat.Client.Core.ApiResponse<CategoryDto> { IsSuccess = true, Data = new CategoryDto { Id = 5, Name = "new-cat" } });

        var parameters = new DialogParameters { { "ServerId", 1 } };
        IDialogReference? dialogReference = null;
        
        await provider.InvokeAsync(async () =>
        {
            dialogReference = await dialogService.ShowAsync<CreateCategoryDialog>("Create", parameters);
        });

        Assert.That(provider.Markup, Does.Contain("Create Category"));

        var input = provider.Find("input");
        await provider.InvokeAsync(() => input.Input("new-cat"));

        var submitBtn = provider.FindAll("button").FirstOrDefault(b => b.InnerHtml.Contains("Create Category"));
        Assert.That(submitBtn, Is.Not.Null);

        await provider.InvokeAsync(() => submitBtn!.Click());

        var result = await dialogReference!.Result;
        Assert.That(result!.Canceled, Is.False);
        var dto = result.Data as CategoryDto;
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(5));
    }
}
