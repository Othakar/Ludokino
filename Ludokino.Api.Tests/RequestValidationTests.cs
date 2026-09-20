using System.ComponentModel.DataAnnotations;
using Ludokino.Api.DTOs.Articles;
using Xunit;

namespace Ludokino.Api.Tests;

public class RequestValidationTests
{
    [Fact]
    public void CreateArticleRequest_WithInvalidImageUrl_ShouldFailValidation()
    {
        var request = new CreateArticleRequest
        {
            Title = "Titre valide",
            Excerpt = "Résumé",
            Content = "Contenu",
            Status = "Draft",
            ImageUrls = ["not-a-url"]
        };

        var isValid = Validate(request, out var results);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateArticleRequest.ImageUrls)));
    }

    [Fact]
    public void UpdateArticleRequest_WithInvalidImageUrl_ShouldFailValidation()
    {
        var request = new UpdateArticleRequest
        {
            ImageUrls = ["ftp://example.com/image.png"]
        };

        var isValid = Validate(request, out var results);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(UpdateArticleRequest.ImageUrls)));
    }

    [Fact]
    public void CreateArticleRequest_WithValidImageUrls_ShouldPassValidation()
    {
        var request = new CreateArticleRequest
        {
            Title = "Titre valide",
            Excerpt = "Résumé",
            Content = "Contenu",
            Status = "Draft",
            ImageUrls = ["https://i.imgur.com/image.png", "http://example.com/image.jpg"]
        };

        var isValid = Validate(request, out _);

        Assert.True(isValid);
    }

    private static bool Validate(object model, out List<ValidationResult> results)
    {
        var context = new ValidationContext(model);
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
    }
}
