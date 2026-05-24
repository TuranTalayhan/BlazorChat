using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NUnit.Framework;

namespace BlazorChat.Tests.Client;

public abstract class BunitTestContext
{
    protected BunitContext TestContext { get; private set; } = null!;

    [SetUp]
    public void Setup()
    {
        TestContext = new BunitContext();
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.Services.AddMudServices();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (TestContext is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            TestContext?.Dispose();
        }
    }
}
