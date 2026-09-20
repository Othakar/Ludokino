using Ludokino.Api.Data;
using Ludokino.Api.Models;
using Ludokino.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Ludokino.Api.Services;

public class RepositoryService : IRepositoryService
{
    private readonly AppDbContext _context;

    public RepositoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetArticlesCountAsync()
    {
        return await _context.Articles.CountAsync(a => a.Status == ArticleStatus.Published);
    }

    public async Task<int> GetDraftsCountAsync()
    {
        return await _context.Articles.CountAsync(a => a.Status == ArticleStatus.Draft);
    }

    public async Task<int> GetVisibleUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsVisibleOnTeamPage);
    }
}
