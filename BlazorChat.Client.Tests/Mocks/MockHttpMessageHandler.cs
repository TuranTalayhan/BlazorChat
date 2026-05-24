using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorChat.Tests.Client.Mocks;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _routes = new();

    public void ExpectGet<T>(string uriEndsWith, T responseData)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = System.Net.Http.Json.JsonContent.Create(responseData) } 
            : null!;
    }

    public void ExpectPost<T>(string uriEndsWith, T responseData)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = System.Net.Http.Json.JsonContent.Create(responseData) } 
            : null!;
    }
    
    public void ExpectDelete(string uriEndsWith)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Delete && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
            : null!;
    }

    public void ExpectPatch(string uriEndsWith)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Patch && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(System.Net.HttpStatusCode.OK) 
            : null!;
    }

    public void ExpectGetError(string uriEndsWith, System.Net.HttpStatusCode status)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(status) 
            : null!;
    }

    public void ExpectPostError(string uriEndsWith, System.Net.HttpStatusCode status)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(status) 
            : null!;
    }

    public void ExpectPostError<T>(string uriEndsWith, System.Net.HttpStatusCode status, T errorData)
    {
        _routes[uriEndsWith] = req => req.Method == HttpMethod.Post && req.RequestUri!.ToString().EndsWith(uriEndsWith) 
            ? new HttpResponseMessage(status) { Content = System.Net.Http.Json.JsonContent.Create(errorData) }
            : null!;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var handler in _routes.Values)
        {
            var res = handler(request);
            if (res != null) return Task.FromResult(res);
        }

        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound) 
        { 
            ReasonPhrase = $"Route not mocked: {request.Method} {request.RequestUri}" 
        });
    }
}
