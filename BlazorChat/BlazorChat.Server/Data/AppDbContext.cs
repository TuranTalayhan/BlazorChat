using BlazorChat.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<ChatServer> Servers { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<ChannelCategory> ChannelCategories { get; set; }
    public DbSet<ServerMembership> ServerMemberships { get; set; }
    public DbSet<Message> Messages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();

        // ── Friendship ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Friendship>()
            .HasKey(f => new { f.RequesterId, f.ReceiverId });

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.SentRequests)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Receiver)
            .WithMany(u => u.ReceivedRequests)
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── ChatServer ─────────────────────────────────────────────────────────
        modelBuilder.Entity<ChatServer>()
            .HasOne(s => s.Owner)
            .WithMany()
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ── ChannelCategory ──────────────────────────────────────
        modelBuilder.Entity<ChannelCategory>(entity =>
        {
            entity.HasOne(cc => cc.Server)
                .WithMany(s => s.Categories)
                .HasForeignKey(cc => cc.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cc => new { cc.ServerId, cc.SortOrder });
        });
        
        // ── Channel ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasOne(c => c.Server)
                .WithMany(s => s.Channels)
                .HasForeignKey(c => c.ServerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasOne(c => c.Category)
                .WithMany(cc => cc.Channels)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(c => new { c.ServerId, c.SortOrder });
            
            entity.HasIndex(c => new { c.CategoryId, c.SortOrder });

            entity.HasMany(c => c.Members)
                .WithMany()
                .UsingEntity(j => j.ToTable("ChannelMembers"));
        });

        // ── ServerMembership ───────────────────────────────────────────────────
        modelBuilder.Entity<ServerMembership>()
            .HasKey(sm => new { sm.ServerId, sm.UserId });

        modelBuilder.Entity<ServerMembership>()
            .HasOne(sm => sm.Server)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ServerMembership>()
            .HasOne(sm => sm.User)
            .WithMany(u => u.ServerMemberships)
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Message ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Channel)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.ChannelId, m.CreatedAt });
    }
}