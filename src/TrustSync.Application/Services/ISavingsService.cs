using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface ISavingsService
{
    Task<IReadOnlyList<SavingGoalDto>> GetAllGoalsAsync();
    Task<Result<SavingGoalDto>> CreateGoalAsync(SavingGoalCreateDto dto);
    Task<Result<SavingGoalDto>> UpdateGoalAsync(SavingGoalUpdateDto dto);
    Task<Result> DeleteGoalAsync(int id);
    Task<IReadOnlyList<SavingEntryDto>> GetEntriesAsync(int goalId);
    Task<Result<SavingEntryDto>> AddEntryAsync(SavingEntryCreateDto dto);
    Task<Result> DeleteEntryAsync(int id);
}
