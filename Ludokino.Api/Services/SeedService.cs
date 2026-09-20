using BCrypt.Net;
using Ludokino.Api.Data;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class SeedService : ISeedService
{
    private readonly AppDbContext _context;

    public SeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        if (await _context.Roles.AnyAsync())
        {
            return;
        }

        var adminRole = new Role { Name = "Admin", Description = "Administrateur" };
        var redacteurRole = new Role { Name = "Redacteur", Description = "Rédacteur" };
        _context.Roles.AddRange(adminRole, redacteurRole);
        await _context.SaveChangesAsync();

        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var editorPasswordHash = BCrypt.Net.BCrypt.HashPassword("editor123");

        _context.Users.AddRange(
            new User
            {
                Name = "Admin",
                Pseudo = "Ludokino Admin",
                Email = "admin@ludokino.fr",
                PasswordHash = adminPasswordHash,
                RoleId = adminRole.Id,
                IsVisibleOnTeamPage = true,
                Bio = "Administrateur du site",
                AvatarUrl = "https://placehold.co/600x600/111827/ffffff/png?text=Admin"
            },
            new User
            {
                Name = "Rédacteur",
                Pseudo = "Ludokino Redacteur",
                Email = "redacteur@ludokino.fr",
                PasswordHash = editorPasswordHash,
                RoleId = redacteurRole.Id,
                IsVisibleOnTeamPage = true,
                Bio = "Rédacteur en chef",
                AvatarUrl = "https://placehold.co/600x600/111827/ffffff/png?text=Editor"
            }
        );

        await _context.SaveChangesAsync();
    }
}
