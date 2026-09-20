using Ludokino.Api.DTOs.Emissions;

namespace Ludokino.Api.Services.Interfaces;

public interface IEmissionService
{
    Task<List<EmissionDto>> GetAllAsync();
    Task<List<EmissionDto>> GetFeaturedAsync();
    Task<EmissionDto?> GetBySlugAsync(string slug);
    Task<EmissionDto> CreateAsync(CreateEmissionRequest request);
    Task<EmissionDto?> UpdateAsync(int id, UpdateEmissionRequest request);
    Task<bool> DeleteAsync(int id);
}
