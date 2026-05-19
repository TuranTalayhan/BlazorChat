using BlazorChat.Server.Application.Features.Servers.Handlers;
using BlazorChat.Server.Application.Features.Servers.Queries;
using BlazorChat.Server.Domain.Entities;
using BlazorChat.Server.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlazorChat.Tests.Server.Application.Features.Servers.Handlers;

public class GetMyServersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasServers_ShouldReturnOnlyTheirServers()
    {
       
    }

    private static AppDbContext CreateTestDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}