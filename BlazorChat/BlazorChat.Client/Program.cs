using BlazorChat.Client;
using BlazorChat.Client.Features.Authentication;
using BlazorChat.Client.Features.Authentication.ViewModels;
using BlazorChat.Client.Features.Chat.Services;
using BlazorChat.Client.Features.Friends;
using BlazorChat.Client.Features.Notifications.ViewModel;
using BlazorChat.Client.Features.Servers;
using BlazorChat.Client.Features.Servers.Dialogs.AddServer;
using BlazorChat.Client.Features.Servers.Services;
using BlazorChat.Client.Features.User;
using BlazorChat.Client.Features.User.Services;
using BlazorChat.Client.Services;
using BlazorChat.Client.Services.Chat;
using BlazorChat.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Mediator;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register the custom handler
builder.Services.AddTransient<CookieHandler>();

// Register the HttpClient using the custom handler
builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<CookieHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    return new HttpClient(handler) 
    { 
        BaseAddress = new Uri("http://localhost:7138") 
    };
});

builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();
builder.Services.AddMediator(options => 
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
});


builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<ChatViewModel>();
builder.Services.AddScoped<ChatAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ChatAuthStateProvider>());
builder.Services.AddScoped<IFriendshipApiService, FriendshipApiService>();
builder.Services.AddScoped<IChatHubService, ChatHubService>();
builder.Services.AddTransient<FriendsSidebarViewModel>();
builder.Services.AddScoped<IServerApiService, ServerApiService>();
builder.Services.AddScoped<UserProfileViewModel>();
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IChatApiService, ChatApiService>();
builder.Services.AddScoped<IServerApiService, ServerApiService>();
builder.Services.AddScoped<SidebarViewModel>();
builder.Services.AddScoped<IChannelApiService, ChannelApiService>();
builder.Services.AddScoped<NavigationState>();
builder.Services.AddScoped<TopBarViewModel>();
builder.Services.AddScoped<ServerChannelsViewModel>();
builder.Services.AddScoped<NotificationInboxViewModel>();
builder.Services.AddScoped<CreateServerViewModel>();

await builder.Build().RunAsync();
