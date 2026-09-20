using Ludokino.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludokino.Api.Tests;

public class PostgresSchemaTests
{
    [PostgreSqlFact]
    public async Task TestDatabase_UsesCurrentEfCoreMigrations()
    {
        var connectionString = Environment.GetEnvironmentVariable("LUDOKINO_TEST_CONNECTION_STRING");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        Assert.Empty(pendingMigrations);
        Assert.True(await context.Database.CanConnectAsync());
    }
}