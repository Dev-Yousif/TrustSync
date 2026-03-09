using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync();
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<Result<ProjectDto>> CreateAsync(ProjectCreateDto dto);
    Task<Result<ProjectDto>> UpdateAsync(ProjectUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
