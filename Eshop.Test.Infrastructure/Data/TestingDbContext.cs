
using EShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Test.Infrastructure.Data;

public sealed class TestingDbContext : ApplicationDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQLite in in-memory mode with a unique identifier to isolate test instances
        optionsBuilder.UseSqlite("DataSource=:memory:");
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
    }

    public async Task InitializeAsync()
    {
        // Open the connection and create the database schema
        await Database.OpenConnectionAsync();
        await Database.EnsureCreatedAsync();
    }

    public override void Dispose()
    {
        Database.CloseConnection();  // Close the SQLite connection when done
        base.Dispose();
    }
}