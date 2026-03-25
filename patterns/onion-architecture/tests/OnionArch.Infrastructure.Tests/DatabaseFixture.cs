using Microsoft.EntityFrameworkCore;
using OnionArch.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace OnionArch.Infrastructure.Tests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("onionarch_test")
        .WithUsername("test")
        .WithPassword("test123")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.Migrate();
        return context;
    }
}
