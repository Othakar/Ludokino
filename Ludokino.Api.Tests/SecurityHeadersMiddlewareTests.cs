using Ludokino.Api.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Ludokino.Api.Tests;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Production_AddsSecurityHeaders()
    {
        var context = new DefaultHttpContext();
        var environment = CreateEnvironment(Environments.Production);
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, environment.Object);

        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("camera=(), microphone=(), geolocation=()", context.Response.Headers["Permissions-Policy"]);
        Assert.Equal("max-age=31536000; includeSubDomains", context.Response.Headers["Strict-Transport-Security"]);
        Assert.Contains("frame-ancestors 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
    }

    [Fact]
    public async Task Development_DoesNotAddProductionOnlyHeaders()
    {
        var context = new DefaultHttpContext();
        var environment = CreateEnvironment(Environments.Development);
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, environment.Object);

        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal(0, context.Response.Headers["Strict-Transport-Security"].Count);
        Assert.Equal(0, context.Response.Headers["Content-Security-Policy"].Count);
    }

    private static Mock<IWebHostEnvironment> CreateEnvironment(string environmentName)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.EnvironmentName).Returns(environmentName);
        return environment;
    }
}
