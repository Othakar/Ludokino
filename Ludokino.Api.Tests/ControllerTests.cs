using System.Reflection;
using Ludokino.Api.DTOs.Articles;
using Ludokino.Api.Controllers;
using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task ArticlesController_GetById_ReturnsArticleForAdminEdit()
    {
        var service = new Mock<IArticleService>();
        service.Setup(item => item.GetByIdAsync(42, true))
            .ReturnsAsync(new ArticleDto
            {
                Id = 42,
                Slug = "article-42",
                Title = "Article 42",
                Content = "Contenu détaillé",
                Status = "Draft"
            });
        var controller = new ArticlesController(service.Object);

        var result = await controller.GetById(42);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var article = Assert.IsType<ArticleDto>(okResult.Value);
        Assert.Equal(42, article.Id);
        Assert.Equal("Article 42", article.Title);
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

    [Fact]
    public void Controllers_ShouldNotDependOnDbContext()
    {
        var controllerTypes = typeof(ArticlesController).Assembly
            .GetTypes()
            .Where(type =>
                typeof(ControllerBase).IsAssignableFrom(type) &&
                type.IsClass &&
                !type.IsAbstract &&
                type.Namespace == typeof(ArticlesController).Namespace);

        foreach (var controllerType in controllerTypes)
        {
            foreach (var constructor in controllerType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    Assert.False(typeof(AppDbContext).IsAssignableFrom(parameter.ParameterType),
                        $"{controllerType.Name} ne doit pas dépendre de {nameof(AppDbContext)}.");
                    Assert.False(typeof(DbContext).IsAssignableFrom(parameter.ParameterType),
                        $"{controllerType.Name} ne doit pas dépendre directement de {nameof(DbContext)}.");
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(SensitiveEndpoints))]
    public void SensitiveEndpoints_ShouldRequireExpectedAuthorization(Type controllerType, string methodName, string? expectedRoles)
    {
        var method = controllerType.GetMethod(methodName);
        Assert.NotNull(method);

        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>() ??
                        controllerType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);

        if (expectedRoles is null)
        {
            return;
        }

        Assert.Equal(NormalizeRoles(expectedRoles), NormalizeRoles(authorize!.Roles));
    }

    public static TheoryData<Type, string, string?> SensitiveEndpoints => new()
    {
        { typeof(AuthController), nameof(AuthController.GetMe), null },
        { typeof(ArticlesController), nameof(ArticlesController.GetDrafts), "Admin,Redacteur" },
        { typeof(ArticlesController), nameof(ArticlesController.Create), "Admin,Redacteur" },
        { typeof(ArticlesController), nameof(ArticlesController.Update), "Admin,Redacteur" },
        { typeof(ArticlesController), nameof(ArticlesController.Delete), "Admin" },
        { typeof(CategoriesController), nameof(CategoriesController.Create), "Admin,Redacteur" },
        { typeof(CategoriesController), nameof(CategoriesController.Update), "Admin,Redacteur" },
        { typeof(CategoriesController), nameof(CategoriesController.Delete), "Admin" },
        { typeof(TagsController), nameof(TagsController.Create), "Admin,Redacteur" },
        { typeof(TagsController), nameof(TagsController.Update), "Admin,Redacteur" },
        { typeof(TagsController), nameof(TagsController.Delete), "Admin" },
        { typeof(EmissionsController), nameof(EmissionsController.Create), "Admin,Redacteur" },
        { typeof(EmissionsController), nameof(EmissionsController.Update), "Admin,Redacteur" },
        { typeof(EmissionsController), nameof(EmissionsController.Delete), "Admin" },
        { typeof(EmissionsController), nameof(EmissionsController.SynchronizeYoutube), "Admin" },
        { typeof(TeamController), nameof(TeamController.GetAllMembers), "Admin,Redacteur" },
        { typeof(DashboardController), nameof(DashboardController.GetStats), "Admin,Redacteur" }
    };

    private static string NormalizeRoles(string? roles)
    {
        return string.Join(',',
            (roles ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(static role => role, StringComparer.Ordinal));
    }
}