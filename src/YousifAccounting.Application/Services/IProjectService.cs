using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetAllAsync();
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<Result<ProjectDto>> CreateAsync(ProjectCreateDto dto);
    Task<Result<ProjectDto>> UpdateAsync(ProjectUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
