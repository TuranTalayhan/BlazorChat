using System;
using System.Linq;
using BlazorChat.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorChat.Tests.Server.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<BlazorChat.Server.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all EF Core and Npgsql related services to prevent provider conflicts
            var efServices = services.Where(d => 
                d.ServiceType.FullName != null && 
                (d.ServiceType.FullName.Contains("EntityFrameworkCore") || 
                 d.ServiceType.FullName.Contains("Npgsql") ||
                 d.ServiceType == typeof(AppDbContext) ||
                 d.ServiceType.Name.Contains("DbContextOptions")))
                .ToList();
            
            foreach (var d in efServices)
            {
                services.Remove(d);
            }

            var dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
            

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            // Remove existing authentication schemes and add our test scheme
            services.AddAuthentication(defaultScheme: TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });
                    
            services.Configure<AuthenticationOptions>(o =>
            {
                o.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                o.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            });
            
            services.AddAuthorization(options =>
            {
                var builder = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(TestAuthHandler.AuthenticationScheme);
                builder.RequireAuthenticatedUser();
                options.DefaultPolicy = builder.Build();
            });
                    
            // We need to ensure the DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
