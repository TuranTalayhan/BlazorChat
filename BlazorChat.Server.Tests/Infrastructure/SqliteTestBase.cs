using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BlazorChat.Tests.Server.Infrastructure;

public abstract class SqliteTestBase<TDbContext> where TDbContext : DbContext
{
    private SqliteConnection _connection = null!;
    protected TDbContext Context { get; private set; } = null!;

    [SetUp]
    public void InitializeSqliteDatabase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;

        Context.Database.EnsureCreated();
    }

    [TearDown]
    public void DisposeSqliteDatabase()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}