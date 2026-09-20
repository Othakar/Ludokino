using Ludokino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Emission> Emissions => Set<Emission>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleAuthor> ArticleAuthors => Set<ArticleAuthor>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<ArticleEmission> ArticleEmissions => Set<ArticleEmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<SocialLink>()
            .HasOne(s => s.User)
            .WithMany(u => u.SocialLinks)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Slug)
            .IsUnique();

        modelBuilder.Entity<ArticleAuthor>()
            .HasOne(aa => aa.Article)
            .WithMany(a => a.ArticleAuthors)
            .HasForeignKey(aa => aa.ArticleId);

        modelBuilder.Entity<ArticleAuthor>()
            .HasOne(aa => aa.User)
            .WithMany(u => u.ArticleAuthors)
            .HasForeignKey(aa => aa.UserId);

        modelBuilder.Entity<ArticleCategory>()
            .HasOne(ac => ac.Article)
            .WithMany(a => a.ArticleCategories)
            .HasForeignKey(ac => ac.ArticleId);

        modelBuilder.Entity<ArticleCategory>()
            .HasOne(ac => ac.Category)
            .WithMany(c => c.ArticleCategories)
            .HasForeignKey(ac => ac.CategoryId);

        modelBuilder.Entity<ArticleTag>()
            .HasOne(at => at.Article)
            .WithMany(a => a.ArticleTags)
            .HasForeignKey(at => at.ArticleId);

        modelBuilder.Entity<ArticleTag>()
            .HasOne(at => at.Tag)
            .WithMany(t => t.ArticleTags)
            .HasForeignKey(at => at.TagId);

        modelBuilder.Entity<ArticleEmission>()
            .HasOne(ae => ae.Article)
            .WithMany(a => a.ArticleEmissions)
            .HasForeignKey(ae => ae.ArticleId);

        modelBuilder.Entity<ArticleEmission>()
            .HasOne(ae => ae.Emission)
            .WithMany(e => e.ArticleEmissions)
            .HasForeignKey(ae => ae.EmissionId);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<Emission>()
            .HasIndex(e => e.Slug)
            .IsUnique();
    }
}
