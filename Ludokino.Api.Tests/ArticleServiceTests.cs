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
