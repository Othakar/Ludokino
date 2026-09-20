namespace Ludokino.Api.Models;

public class SocialLink
{
    public int Id { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
