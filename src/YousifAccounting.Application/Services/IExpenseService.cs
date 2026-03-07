using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync();
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync();
    Task<Result<ExpenseDto>> CreateAsync(ExpenseCreateDto dto);
    Task<Result<ExpenseDto>> UpdateAsync(ExpenseUpdateDto dto);
    Task<Result> DeleteAsync(int id);
}
