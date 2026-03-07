using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface IDeductionService
{
    Task<IReadOnlyList<DeductionDto>> GetAllAsync();
    Task<Result<DeductionDto>> CreateAsync(DeductionCreateDto dto);
    Task<Result<DeductionDto>> UpdateAsync(DeductionUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
