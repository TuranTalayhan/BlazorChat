using BlazorChat.Server.Application.Interfaces;
using BlazorChat.Server.Context;
using BlazorChat.Server.Hubs;
using BlazorChat.Server.Infrastructure.Persistence;
using BlazorChat.Server.Infrastructure.Persistence.Entities;
using BlazorChat.Server.Infrastructure.Services;
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
        
        string[] clientOrigins;
        var originsSection = builder.Configuration.GetSection("ClientOrigins");
        if (originsSection.Exists())
        {
            if (originsSection.Value is null)
            {
                clientOrigins = originsSection.Get<string[]>() ?? [];
            }
            else
            {
                clientOrigins = originsSection.Value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
        else
        {
            clientOrigins = ["http://localhost:5173"];
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
                policy.WithOrigins(clientOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
        
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth_token";
                options.LoginPath = "/"; // Where to send users if they aren't logged in
                options.Cookie.MaxAge = TimeSpan.FromDays(7);
                
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api") || 
                        context.Request.Path.StartsWithSegments("/hubs"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
            
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });
        
        builder.Services.AddAuthorization();
        
        // Add services to the container.
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();
        builder.Services.AddScoped<IFriendNotificationService, FriendNotificationService>();
        builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
        builder.Services.AddScoped<IChannelAuthorizationService, ChannelAuthorizationService>();
        builder.Services.AddScoped<ICategoryManager, CategoryManager>();
        builder.Services.AddMediator(options => 
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
        

        var app = builder.Build();

        // Dev/demo: wipe + recreate schema + seed on every startup
        if (app.Environment.IsDevelopment())
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();

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

                // ── Seed Dev Server ───────────────────────────────────────────────────
                var devServer = new ChatServer { Name = "Dev Server", OwnerId = alice.Id };
                context.Servers.Add(devServer);
                await context.SaveChangesAsync();

                // ── Seed Categories ───────────────────────────────────────────────────
                var textCategory = new ChannelCategory { Name = "TEXT CHANNELS", ServerId = devServer.Id, SortOrder = 0 };
                var gamingCategory = new ChannelCategory { Name = "GAMING", ServerId = devServer.Id, SortOrder = 1 };
                context.ChannelCategories.AddRange(textCategory, gamingCategory);
                await context.SaveChangesAsync();

                // ── Seed Channels ─────────────────────────────────────────────────────
                var generalChannel = new Channel { Name = "general", ServerId = devServer.Id, CategoryId = textCategory.Id, SortOrder = 0 };
                var randomChannel = new Channel { Name = "random", ServerId = devServer.Id, CategoryId = textCategory.Id, SortOrder = 1 };
                
                var lobbyChannel = new Channel { Name = "lobby", ServerId = devServer.Id, CategoryId = gamingCategory.Id, SortOrder = 0 };
                var patchNotes = new Channel { Name = "patch-notes", ServerId = devServer.Id, CategoryId = gamingCategory.Id, SortOrder = 1 };
                
                // A root-level channel (no category)
                var rulesChannel = new Channel { Name = "rules", ServerId = devServer.Id, CategoryId = null, SortOrder = 0 };

                context.Channels.AddRange(generalChannel, randomChannel, lobbyChannel, patchNotes, rulesChannel);
                await context.SaveChangesAsync();

                // ── Memberships & Messages ───────────────────────────────────────────
                context.ServerMemberships.AddRange(
                    new ServerMembership { ServerId = devServer.Id, UserId = alice.Id, Role = ServerRole.Owner },
                    new ServerMembership { ServerId = devServer.Id, UserId = bob.Id, Role = ServerRole.Member },
                    new ServerMembership { ServerId = devServer.Id, UserId = carol.Id, Role = ServerRole.Member }
                );

                context.Messages.AddRange(
                    new Message { Content = "Welcome to #general!", AuthorId = alice.Id, ChannelId = generalChannel.Id },
                    new Message { Content = "Check the #rules before posting.", AuthorId = alice.Id, ChannelId = generalChannel.Id },
                    new Message { Content = "Anyone up for a game in #lobby?", AuthorId = bob.Id, ChannelId = lobbyChannel.Id }
                );

                // ── Direct Messages (No Category/Server) ──────────────────────────────
                var dmChannel = new Channel 
                { 
                    Type = ChannelType.DirectMessage, 
                    Members = new List<User> { alice, bob }
                };
                context.Channels.Add(dmChannel);
                await context.SaveChangesAsync();

                context.Messages.Add(
                    new Message { Content = "Hey Bob, DM works!", AuthorId = alice.Id, ChannelId = dmChannel.Id }
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
        
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapHub<FriendHub>("/hubs/friend");
        app.MapHub<UserHub>("/hubs/user");
        app.MapHub<ServerHub>("/hubs/server");

        await app.RunAsync();
    }
}
