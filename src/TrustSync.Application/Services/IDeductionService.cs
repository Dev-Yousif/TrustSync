using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface IDeductionService
{
    Task<IReadOnlyList<DeductionDto>> GetAllAsync();
    Task<Result<DeductionDto>> CreateAsync(DeductionCreateDto dto);
    Task<Result<DeductionDto>> UpdateAsync(DeductionUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
