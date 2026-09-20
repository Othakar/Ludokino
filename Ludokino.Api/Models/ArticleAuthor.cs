namespace Ludokino.Api.Models;

public class ArticleAuthor
{
    public int Id { get; set; }
    public string Role { get; set; } = "author";
    public bool IsPrimary { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
