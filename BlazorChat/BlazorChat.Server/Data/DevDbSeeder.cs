using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Data;

public static class DevDbSeeder
{
    public static void EnsureSeedData(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<AppDbContext>();

        // Apply migrations
        context.Database.Migrate();

        // Seed only if no users
        if (context.Users.Any()) return;

        var users = new[]
        {
            new Data.Entities.User { Username = "alice", Email = "alice@example.com", PasswordHash = "nopass", Status = Shared.DTO.UserStatus.Online },
            new Data.Entities.User { Username = "bob", Email = "bob@example.com", PasswordHash = "nopass", Status = Shared.DTO.UserStatus.Offline },
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Optional: seed a welcome message
        if (!context.Messages.Any())
        {
            context.Messages.Add(new Data.Entities.Message
            {
                AuthorId = users[0].Id,
                ChannelId = 1,
                Content = "Welcome to BlazorChat!",
            });
            context.SaveChanges();
        }
    }
}
