using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Services;
using Xunit;

namespace Ludokino.Api.Tests;

public class EmissionServiceTests
{
    [Fact]
    public async Task CreateAsync_WithoutYoutubeUrl_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show"
        }));
    }

    [Fact]
    public async Task CreateAsync_WithNonYoutubeUrl_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show",
            YoutubeUrl = "https://example.com/video"
        }));
    }

    [Fact]
    public async Task CreateAsync_WithYoutubeUrl_ReturnsYoutubeUrl()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new EmissionService(context);

        var emission = await service.CreateAsync(new CreateEmissionRequest
        {
            Name = "Ludokino Show",
            YoutubeUrl = "https://youtu.be/example"
        });

        Assert.Equal("https://youtu.be/example", emission.YoutubeUrl);
    }
}