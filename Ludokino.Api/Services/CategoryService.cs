using System.Text.RegularExpressions;
using Ludokino.Api.Data;
using Ludokino.Api.DTOs.Categories;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => Map(c))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug.Trim());

        return category is null ? null : Map(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        ValidateName(request.Name);
        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = await EnsureUniqueSlugAsync(CreateSlug(request.Name))
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Map(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) return null;

        var name = request.Name?.Trim();
        ValidateName(name);
        category.Name = name!;
        category.Slug = await EnsureUniqueSlugAsync(CreateSlug(category.Name), id);
        await _context.SaveChangesAsync();
        return Map(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, int? excludedId = null)
    {
        var candidate = slug;
        var index = 1;
        while (await _context.Categories.AnyAsync(c => c.Slug == candidate && (!excludedId.HasValue || c.Id != excludedId.Value)))
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
        return string.IsNullOrWhiteSpace(slug) ? "category" : slug;
    }

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Le nom est obligatoire.");
    }

    private static CategoryDto Map(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug
    };
}
