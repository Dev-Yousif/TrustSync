using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface IIncomeService
{
    Task<IReadOnlyList<IncomeDto>> GetAllAsync();
    Task<Result<IncomeDto>> CreateAsync(IncomeCreateDto dto);
    Task<Result<IncomeDto>> UpdateAsync(IncomeUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
