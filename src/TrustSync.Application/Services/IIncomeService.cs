using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface IIncomeService
{
    Task<IReadOnlyList<IncomeDto>> GetAllAsync();
    Task<Result<IncomeDto>> CreateAsync(IncomeCreateDto dto);
    Task<Result<IncomeDto>> UpdateAsync(IncomeUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
