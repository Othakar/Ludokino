namespace Ludokino.Api.Models;

public class ArticleEmission
{
    public int Id { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int EmissionId { get; set; }
    public Emission Emission { get; set; } = null!;
}
