using BlazorChat.Server.Data;
using BlazorChat.Server.Data.Entities;
using BlazorChat.Shared.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth_token";
                options.LoginPath = "/"; // Where to send users if they aren't logged in
                options.Cookie.MaxAge = TimeSpan.FromDays(7);
            });
        
        builder.Services.AddAuthorization();
        
        // Add services to the container.
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        // CORS: allow client origins configured via configuration key "ClientOrigins"
        // Support either a semicolon-separated string or an array in configuration.
        string[] clientOrigins;
        var originsSection = builder.Configuration.GetSection("ClientOrigins");
        if (originsSection.Exists())
        {
            // If configured as array, bind it; otherwise fall back to semicolon-separated string
            if (originsSection.Value is null)
            {
                clientOrigins = originsSection.Get<string[]>() ?? Array.Empty<string>();
            }
            else
            {
                clientOrigins = originsSection.Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
        else
        {
            clientOrigins = new[] { "http://localhost:5173" };
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
                policy.WithOrigins(clientOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        var app = builder.Build();

        // Dev/demo: wipe + recreate schema + seed on every startup
        if (app.Environment.IsDevelopment())
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var hasher = new PasswordHasher<User>();

                var alice = new User { Email = "alice@example.com", Username = "alice", Status = UserStatus.Online };
                alice.PasswordHash = hasher.HashPassword(alice, "devpassword");

                var bob = new User { Email = "bob@example.com", Username = "bob", Status = UserStatus.Online };
                bob.PasswordHash = hasher.HashPassword(bob, "devpassword");

                var carol = new User { Email = "carol@example.com", Username = "carol", Status = UserStatus.Offline };
                carol.PasswordHash = hasher.HashPassword(carol, "devpassword");

                context.Users.AddRange(alice, bob, carol);
                await context.SaveChangesAsync();

                context.Friendships.Add(new Friendship
                {
                    RequesterId = alice.Id, ReceiverId = bob.Id,
                    Status = FriendshipStatus.Accepted, CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();

                // Seed a dev server with two channels
                var devServer = new Data.Entities.ChatServer { Name = "Dev Server", OwnerId = alice.Id };
                context.Servers.Add(devServer);
                await context.SaveChangesAsync();

                var generalChannel = new Data.Entities.Channel { Name = "general", ServerId = devServer.Id, SortOrder = 0 };
                var randomChannel  = new Data.Entities.Channel { Name = "random",  ServerId = devServer.Id, SortOrder = 1 };
                context.Channels.AddRange(generalChannel, randomChannel);

                context.ServerMemberships.AddRange(
                    new Data.Entities.ServerMembership { ServerId = devServer.Id, UserId = alice.Id, Role = Data.Entities.ServerRole.Owner },
                    new Data.Entities.ServerMembership { ServerId = devServer.Id, UserId = bob.Id,   Role = Data.Entities.ServerRole.Member }
                );
                await context.SaveChangesAsync();

                context.Messages.AddRange(
                    new Message { Content = "Welcome to #general!", AuthorId = alice.Id, ChannelId = generalChannel.Id, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                    new Message { Content = "Hi Alice — glad to be here!", AuthorId = bob.Id, ChannelId = generalChannel.Id, CreatedAt = DateTime.UtcNow },
                    new Message { Content = "Anyone in #random?", AuthorId = alice.Id, ChannelId = randomChannel.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
                );

                // Seed a DM conversation between alice and bob
                var dmConversation = new Data.Entities.DirectMessage { User1Id = alice.Id, User2Id = bob.Id };
                context.DirectMessages.Add(dmConversation);
                await context.SaveChangesAsync();

                context.Messages.Add(
                    new Message { Content = "Hey Bob, DM works!", AuthorId = alice.Id, DirectMessageId = dmConversation.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-2) }
                );
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database seeding failed: {ex.Message}");
            }
        }

        app.UseRouting();
        app.UseCors("AllowClient");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<Hubs.ChatHub>("/hubs/chat");

        app.Run();
    }
}
