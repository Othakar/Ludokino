namespace Ludokino.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
}
