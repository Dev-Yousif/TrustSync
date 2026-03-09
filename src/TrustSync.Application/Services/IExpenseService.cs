using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync();
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync();
    Task<Result<ExpenseDto>> CreateAsync(ExpenseCreateDto dto);
    Task<Result<ExpenseDto>> UpdateAsync(ExpenseUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
