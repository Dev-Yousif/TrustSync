using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface ICompanyClientService
{
    Task<IReadOnlyList<CompanyClientDto>> GetAllAsync();
    Task<CompanyClientDto?> GetByIdAsync(int id);
    Task<Result<CompanyClientDto>> CreateAsync(CompanyClientCreateDto dto);
    Task<Result<CompanyClientDto>> UpdateAsync(CompanyClientUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
