using System.Reflection;
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
        var controller = new EmissionsController(service.Object);

        var result = await controller.Create(new CreateEmissionRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EmissionsController_GetAll_ReturnsOk()
    {
        var service = new Mock<IEmissionService>();
        service.Setup(item => item.GetAllAsync()).ReturnsAsync(new List<EmissionDto>());
        var controller = new EmissionsController(service.Object);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<List<EmissionDto>>(okResult.Value);
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