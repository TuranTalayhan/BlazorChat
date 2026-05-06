using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorChat.Client;

public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // This is the Blazor WebAssembly equivalent of JavaScript's `credentials: 'include'`
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}