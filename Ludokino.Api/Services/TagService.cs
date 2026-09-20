using System.Text.RegularExpressions;
using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Tags;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _context;

    public TagService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => Map(t))
            .ToListAsync();
    }

    public async Task<TagDto?> GetBySlugAsync(string slug)
    {
        var tag = await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug.Trim());

        return tag is null ? null : Map(tag);
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request)
    {
        ValidateName(request.Name);
        var tag = new Tag
        {
            Name = request.Name.Trim(),
            Slug = await EnsureUniqueSlugAsync(CreateSlug(request.Name))
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return Map(tag);
    }

    public async Task<TagDto?> UpdateAsync(int id, UpdateTagRequest request)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag is null) return null;

        var name = request.Name?.Trim();
        ValidateName(name);
        tag.Name = name!;
        tag.Slug = await EnsureUniqueSlugAsync(CreateSlug(tag.Name), id);
        await _context.SaveChangesAsync();
        return Map(tag);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag is null) return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int? excludedId = null)
    {
        var candidate = slug;
        var index = 1;
        while (await _context.Tags.AnyAsync(t => t.Slug == candidate && (!excludedId.HasValue || t.Id != excludedId.Value)))
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
        return string.IsNullOrWhiteSpace(slug) ? "tag" : slug;
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Le nom est obligatoire.");
    }

    private static TagDto Map(Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        Slug = tag.Slug
    };
}
