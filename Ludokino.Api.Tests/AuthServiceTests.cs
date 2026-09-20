using Ludokino.Api.DTOs.Auth;
using Ludokino.Api.Models;
using Ludokino.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Ludokino.Api.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var role = new Role { Id = 1, Name = "Admin" };
        context.Roles.Add(role);
        context.Users.Add(new User
        {
            Id = 1,
            Name = "Admin",
            Pseudo = "Ludokino Admin",
            Email = "admin@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
            RoleId = role.Id,
            Role = role
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration(), CreateEnvironment());
        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@test.local",
            Password = "secret"
        });

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal("Admin", response.User.Role);
        Assert.Equal("admin@test.local", response.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var role = new Role { Id = 1, Name = "Admin" };
        context.Roles.Add(role);
        context.Users.Add(new User
        {
            Id = 1,
            Name = "Admin",
            Pseudo = "Ludokino Admin",
            Email = "admin@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret"),
            RoleId = role.Id,
            Role = role
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration(), CreateEnvironment());
        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "admin@test.local",
            Password = "wrong-password"
        });

        Assert.Null(response);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-secret-key-that-is-long-enough-123456",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience"
            })
            .Build();
    }

    private static IHostEnvironment CreateEnvironment()
    {
        return new TestHostEnvironment { EnvironmentName = Environments.Development };
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Ludokino.Api.Tests";
        public string ContentRootPath { get; set; } = "/";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
