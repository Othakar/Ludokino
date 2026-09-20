using Ludokino.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Tests;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemory()
    {
        return CreateInMemory(Guid.NewGuid().ToString());
    }

    public static AppDbContext CreateInMemory(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }
}
