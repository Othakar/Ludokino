using Ludokino.Api.DTOs.Articles;
using Ludokino.Api.Models;
using Ludokino.Api.Services;
using Xunit;

namespace Ludokino.Api.Tests;

public class ArticleServiceTests
{
    [Fact]
    public async Task CreateAsync_GeneratesUniqueSlugs()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new ArticleService(context);

        var first = await service.CreateAsync(CreateRequest("Mon article"));
        var second = await service.CreateAsync(CreateRequest("Mon article"));

        Assert.Equal("mon-article", first.Slug);
        Assert.Equal("mon-article-1", second.Slug);
    }

    [Fact]
    public async Task GetPublishedAsync_ExcludesDrafts()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new ArticleService(context);

        await service.CreateAsync(CreateRequest("Brouillon", "Draft"));
        await service.CreateAsync(CreateRequest("Article public", "Published"));

        var published = await service.GetPublishedAsync();

        var article = Assert.Single(published);
        Assert.Equal("Article public", article.Title);
    }

    [Fact]
    public async Task CreateAsync_WithoutTitle_ThrowsArgumentException()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new ArticleService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(CreateRequest(" ")));
    }

    [Fact]
    public async Task CreateAsync_PreservesVideoAndGalleryMetadata()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new ArticleService(context);
        var request = CreateRequest("Article média", "Published");
        request.CoverImageUrl = "https://i.imgur.com/cover.jpeg";
        request.ImageUrls =
        [
            "https://i.imgur.com/gallery-1.jpeg",
            "https://i.imgur.com/gallery-2.jpeg"
        ];
        request.VideoUrl = "https://youtube.com/watch?v=video123";

        var created = await service.CreateAsync(request);

        Assert.Equal(request.CoverImageUrl, created.CoverImageUrl);
        Assert.Equal(request.ImageUrls, created.ImageUrls);
        Assert.Equal(request.VideoUrl, created.VideoUrl);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesGalleryAndVideoMetadata()
    {
        await using var context = TestDbContextFactory.CreateInMemory();
        var service = new ArticleService(context);
        var created = await service.CreateAsync(CreateRequest("Article à enrichir", "Published"));

        var updated = await service.UpdateAsync(created.Id, new UpdateArticleRequest
        {
            ImageUrls = ["https://i.imgur.com/new-image.jpeg", "https://i.imgur.com/new-image.jpeg"],
            VideoUrl = "https://youtu.be/video456"
        });

        Assert.NotNull(updated);
        Assert.Equal(["https://i.imgur.com/new-image.jpeg"], updated!.ImageUrls);
        Assert.Equal("https://youtu.be/video456", updated.VideoUrl);
    }

    private static CreateArticleRequest CreateRequest(string title, string status = "Draft")
    {
        return new CreateArticleRequest
        {
            Title = title,
            Excerpt = "Excerpt",
            Content = "Content",
            Status = status
        };
    }
}
