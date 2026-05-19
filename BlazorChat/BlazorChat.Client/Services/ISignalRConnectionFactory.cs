using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorChat.Client.Services;

public interface ISignalRConnectionFactory
{
    HubConnection CreateConnection(string hubRelativePath);
}

public class SignalRConnectionFactory(NavigationManager navigationManager) : ISignalRConnectionFactory
{
    public HubConnection CreateConnection(string hubRelativePath)
    {
        var baseUri = new Uri(navigationManager.BaseUri);
        
        var absoluteHubUrl = new Uri(baseUri, hubRelativePath).ToString();

        return new HubConnectionBuilder()
            .WithUrl(absoluteHubUrl, options =>
            {
                options.HttpMessageHandlerFactory = innerHandler => 
                    new CookieHandler { InnerHandler = innerHandler };
            })
            .WithAutomaticReconnect()
            .Build();
    }
}