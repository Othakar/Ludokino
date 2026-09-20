namespace Ludokino.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Pseudo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVisibleOnTeamPage { get; set; } = true;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<ArticleAuthor> ArticleAuthors { get; set; } = new List<ArticleAuthor>();
    public ICollection<SocialLink> SocialLinks { get; set; } = new List<SocialLink>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
