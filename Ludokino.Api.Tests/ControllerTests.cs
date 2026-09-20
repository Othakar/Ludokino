using System.Reflection;
using Ludokino.Api.DTOs.Articles;
using Ludokino.Api.Controllers;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ludokino.Api.Tests;

public class ControllerTests
{
    [Fact]
    public async Task CategoriesController_GetBySlug_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(item => item.GetBySlugAsync("missing")).ReturnsAsync((Ludokino.Api.DTOs.Categories.CategoryDto?)null);
        var controller = new CategoriesController(service.Object);

        var result = await controller.GetBySlug("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EmissionsController_Create_WhenServiceRejects_ReturnsBadRequest()
    {
        var service = new Mock<IEmissionService>();
        service
            .Setup(item => item.CreateAsync(It.IsAny<CreateEmissionRequest>()))
            .ThrowsAsync(new ArgumentException("Un lien YouTube HTTPS valide est obligatoire."));
        var controller = new EmissionsController(service.Object, new Mock<IYoutubePlaylistSyncService>().Object);

        var result = await controller.Create(new CreateEmissionRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EmissionsController_GetAll_ReturnsOk()
    {
        var service = new Mock<IEmissionService>();
        service.Setup(item => item.GetAllAsync()).ReturnsAsync(new List<EmissionDto>());
        var controller = new EmissionsController(service.Object, new Mock<IYoutubePlaylistSyncService>().Object);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<EmissionDto>>(okResult.Value);
    }

    [Fact]
    public async Task ArticlesController_GetBySlug_ReturnsArticleMediaContract()
    {
        var service = new Mock<IArticleService>();
        service.Setup(item => item.GetBySlugAsync("media-article", false))
            .ReturnsAsync(new ArticleDto
            {
                Slug = "media-article",
                Content = "Contenu",
                VideoUrl = "https://www.youtube.com/watch?v=video123",
                ImageUrls = ["https://i.imgur.com/gallery.jpeg"]
            });
        var controller = new ArticlesController(service.Object);

        var result = await controller.GetBySlug("media-article");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var article = Assert.IsType<ArticleDto>(okResult.Value);
        Assert.Equal("Contenu", article.Content);
        Assert.Equal("https://www.youtube.com/watch?v=video123", article.VideoUrl);
        Assert.Single(article.ImageUrls);
    }

    [Fact]
    public async Task EmissionsController_SynchronizeYoutube_ReturnsSynchronizedCount()
    {
        var syncService = new Mock<IYoutubePlaylistSyncService>();
        syncService.Setup(item => item.SynchronizeAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        var controller = new EmissionsController(new Mock<IEmissionService>().Object, syncService.Object);

        var result = await controller.SynchronizeYoutube(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var payload = okResult.Value!;
        var synchronizedCount = payload.GetType().GetProperty("synchronizedCount")?.GetValue(payload);
        Assert.Equal(2, synchronizedCount);
    }

    [Fact]
    public void ArticleDelete_RequiresAdminRole()
    {
        var method = typeof(ArticlesController).GetMethod(nameof(ArticlesController.Delete));
        var authorize = method?.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin", authorize.Roles);
    }
}