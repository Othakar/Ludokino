using Ludokino.Api.DTOs.Tags;

namespace Ludokino.Api.Services.Interfaces;

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<TagDto?> GetBySlugAsync(string slug);
    Task<TagDto> CreateAsync(CreateTagRequest request);
    Task<TagDto?> UpdateAsync(int id, UpdateTagRequest request);
    Task<bool> DeleteAsync(int id);
}
