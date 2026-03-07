using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface ICompanyClientService
{
    Task<IReadOnlyList<CompanyClientDto>> GetAllAsync();
    Task<CompanyClientDto?> GetByIdAsync(int id);
    Task<Result<CompanyClientDto>> CreateAsync(CompanyClientCreateDto dto);
    Task<Result<CompanyClientDto>> UpdateAsync(CompanyClientUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
