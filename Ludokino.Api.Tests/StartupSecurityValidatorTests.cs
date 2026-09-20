using Ludokino.Api.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Ludokino.Api.Tests;

public class StartupSecurityValidatorTests
{
    [Fact]
    public void GetCorsOrigins_InProduction_WithoutOrigins_Throws()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Production };

        var action = () => StartupSecurityValidator.GetCorsOrigins(configuration, environment);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void GetCorsOrigins_InProduction_WithHttpOrigin_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://example.com"
            })
            .Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Production };

        var action = () => StartupSecurityValidator.GetCorsOrigins(configuration, environment);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void GetCorsOrigins_InDevelopment_WithoutOrigins_ReturnsLocalhost()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Development };

        var origins = StartupSecurityValidator.GetCorsOrigins(configuration, environment);

        Assert.Single(origins);
        Assert.Equal("http://localhost:3000", origins[0]);
    }

    [Fact]
    public void GetJwtConfiguration_InProduction_WithMissingValues_Throws()
    {
        var configuration = new ConfigurationBuilder().Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Production };

        var action = () => StartupSecurityValidator.GetJwtConfiguration(configuration, environment);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void GetJwtConfiguration_WithShortSecret_Throws()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "short-secret",
                ["JwtSettings:Issuer"] = "LudokinoApi",
                ["JwtSettings:Audience"] = "LudokinoClient"
            })
            .Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Development };

        var action = () => StartupSecurityValidator.GetJwtConfiguration(configuration, environment);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void GetJwtConfiguration_WithValidValues_ReturnsSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-secret-key-that-is-long-enough-123456",
                ["JwtSettings:Issuer"] = "LudokinoApi",
                ["JwtSettings:Audience"] = "LudokinoClient"
            })
            .Build();
        var environment = new FakeHostEnvironment { EnvironmentName = Environments.Development };

        var jwt = StartupSecurityValidator.GetJwtConfiguration(configuration, environment);

        Assert.Equal("test-secret-key-that-is-long-enough-123456", jwt.SecretKey);
        Assert.Equal("LudokinoApi", jwt.Issuer);
        Assert.Equal("LudokinoClient", jwt.Audience);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Ludokino.Api.Tests";
        public string ContentRootPath { get; set; } = "/";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
