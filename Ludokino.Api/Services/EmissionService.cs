using System.Text.RegularExpressions;
using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Emissions;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class EmissionService : IEmissionService
{
    private readonly AppDbContext _context;

    public EmissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmissionDto>> GetAllAsync()
    {
        return await _context.Emissions
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => Map(e))
            .ToListAsync();
    }

    public async Task<List<EmissionDto>> GetFeaturedAsync()
    {
        return await _context.Emissions
            .AsNoTracking()
            .Where(e => e.IsFeatured)
            .OrderBy(e => e.Name)
            .Select(e => Map(e))
            .ToListAsync();
    }

    public async Task<EmissionDto?> GetBySlugAsync(string slug)
    {
        var emission = await _context.Emissions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Slug == slug.Trim());

        return emission is null ? null : Map(emission);
    }

    public async Task<EmissionDto> CreateAsync(CreateEmissionRequest request)
    {
        ValidateName(request.Name);
        ValidateYoutubeUrl(request.YoutubeUrl);
        var emission = new Emission
        {
            Name = request.Name.Trim(),
            Slug = await EnsureUniqueSlugAsync(CreateSlug(request.Name)),
            Description = request.Description,
            Type = request.Type,
            YoutubeUrl = request.YoutubeUrl.Trim(),
            PlaylistUrl = request.PlaylistUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Emissions.Add(emission);
        await _context.SaveChangesAsync();
        return Map(emission);
    }

    public async Task<EmissionDto?> UpdateAsync(int id, UpdateEmissionRequest request)
    {
        var emission = await _context.Emissions.FindAsync(id);
        if (emission is null) return null;

        if (request.Name is not null)
        {
            ValidateName(request.Name);
            emission.Name = request.Name.Trim();
            emission.Slug = await EnsureUniqueSlugAsync(CreateSlug(emission.Name), id);
        }

        if (request.Description is not null) emission.Description = request.Description;
        if (request.Type is not null) emission.Type = request.Type;
        if (request.YoutubeUrl is not null)
        {
            ValidateYoutubeUrl(request.YoutubeUrl);
            emission.YoutubeUrl = request.YoutubeUrl.Trim();
        }
        if (request.PlaylistUrl is not null) emission.PlaylistUrl = request.PlaylistUrl;
        if (request.ThumbnailUrl is not null) emission.ThumbnailUrl = request.ThumbnailUrl;
        if (request.IsFeatured.HasValue) emission.IsFeatured = request.IsFeatured.Value;
        emission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Map(emission);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var emission = await _context.Emissions.FindAsync(id);
        if (emission is null) return false;

        _context.Emissions.Remove(emission);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int? excludedId = null)
    {
        var candidate = slug;
        var index = 1;
        while (await _context.Emissions.AnyAsync(e => e.Slug == candidate && (!excludedId.HasValue || e.Id != excludedId.Value)))
        {
            candidate = $"{slug}-{index++}";
        }

        return candidate;
    }

    private static string CreateSlug(string value)
    {
        var slug = Regex.Replace(value.Trim(), "[\\s_]+", "-");
        slug = Regex.Replace(slug, "[^a-zA-Z0-9-]", "-");
        slug = Regex.Replace(slug, "-+", "-").Trim('-').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(slug) ? "emission" : slug;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Le nom est obligatoire.");
    }

    private static void ValidateYoutubeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            uri.Host is not ("youtube.com" or "www.youtube.com" or "youtu.be"))
        {
            throw new ArgumentException("Un lien YouTube HTTPS valide est obligatoire.");
        }
    }

    private static EmissionDto Map(Emission emission) => new()
    {
        Id = emission.Id,
        Name = emission.Name,
        Slug = emission.Slug,
        Description = emission.Description,
        Type = emission.Type,
        YoutubeUrl = emission.YoutubeUrl,
        PlaylistUrl = emission.PlaylistUrl,
        ImageUrl = emission.ThumbnailUrl
    };
}
