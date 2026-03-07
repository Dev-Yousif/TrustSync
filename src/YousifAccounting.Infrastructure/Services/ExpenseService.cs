using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrencyConversionService _conversion;

    public ExpenseService(AppDbContext db, IAuditService audit, ICurrencyConversionService conversion) { _db = db; _audit = audit; _conversion = conversion; }

    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync()
    {
        return await _db.Expenses
            .Include(e => e.Category)
            .Include(e => e.CompanyClient)
            .Include(e => e.Project)
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                CurrencyCode = e.CurrencyCode,
                Date = e.Date,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColorHex = e.Category != null ? e.Category.ColorHex : null,
                ExpenseType = e.ExpenseType,
                CompanyClientId = e.CompanyClientId,
                CompanyClientName = e.CompanyClient != null ? e.CompanyClient.Name : null,
                ProjectId = e.ProjectId,
                ProjectName = e.Project != null ? e.Project.Name : null,
                IsRecurring = e.IsRecurring,
                RecurrenceType = e.RecurrenceType,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync()
    {
        return await _db.ExpenseCategories
            .OrderBy(c => c.SortOrder)
            .Select(c => new ExpenseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                ColorHex = c.ColorHex,
                IsDefault = c.IsDefault
            })
            .ToListAsync();
    }

    public async Task<Result<ExpenseDto>> CreateAsync(ExpenseCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result<ExpenseDto>.Failure("Description is required.");
        if (dto.Amount <= 0)
            return Result<ExpenseDto>.Failure("Amount must be greater than zero.");
        if (dto.CategoryId <= 0)
            return Result<ExpenseDto>.Failure("Category is required.");

        var entity = new Expense
        {
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            CurrencyCode = dto.CurrencyCode,
            Date = dto.Date,
            CategoryId = dto.CategoryId,
            ExpenseType = dto.ExpenseType,
            CompanyClientId = dto.CompanyClientId,
            ProjectId = dto.ProjectId,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.RecurrenceType,
            Notes = dto.Notes?.Trim()
        };

        var conv = await _conversion.ConvertToDefaultAsync(entity.Amount, entity.CurrencyCode);
        if (conv.IsSuccess)
        {
            entity.ConvertedAmount = conv.Value!.ConvertedAmount;
            entity.ConvertedCurrencyCode = conv.Value.TargetCurrencyCode;
            entity.ExchangeRateUsed = conv.Value.ExchangeRateUsed;
        }
        else
        {
            entity.ConvertedAmount = entity.Amount;
            entity.ConvertedCurrencyCode = entity.CurrencyCode;
            entity.ExchangeRateUsed = 1m;
        }

        _db.Expenses.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Create, "Expense", entity.Id, $"{entity.Description} ({entity.Amount:N2} {entity.CurrencyCode})");

        var items = await GetAllAsync();
        return Result<ExpenseDto>.Success(items.First(e => e.Id == entity.Id));
    }

    public async Task<Result<ExpenseDto>> UpdateAsync(ExpenseUpdateDto dto)
    {
        var entity = await _db.Expenses.FindAsync(dto.Id);
        if (entity is null) return Result<ExpenseDto>.Failure("Expense not found.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result<ExpenseDto>.Failure("Description is required.");
        if (dto.Amount <= 0)
            return Result<ExpenseDto>.Failure("Amount must be greater than zero.");

        entity.Description = dto.Description.Trim();
        entity.Amount = dto.Amount;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.Date = dto.Date;
        entity.CategoryId = dto.CategoryId;
        entity.ExpenseType = dto.ExpenseType;
        entity.CompanyClientId = dto.CompanyClientId;
        entity.ProjectId = dto.ProjectId;
        entity.IsRecurring = dto.IsRecurring;
        entity.RecurrenceType = dto.RecurrenceType;
        entity.Notes = dto.Notes?.Trim();

        var conv = await _conversion.ConvertToDefaultAsync(entity.Amount, entity.CurrencyCode);
        if (conv.IsSuccess)
        {
            entity.ConvertedAmount = conv.Value!.ConvertedAmount;
            entity.ConvertedCurrencyCode = conv.Value.TargetCurrencyCode;
            entity.ExchangeRateUsed = conv.Value.ExchangeRateUsed;
        }
        else
        {
            entity.ConvertedAmount = entity.Amount;
            entity.ConvertedCurrencyCode = entity.CurrencyCode;
            entity.ExchangeRateUsed = 1m;
        }

        await _db.SaveChangesAsync();

        var items = await GetAllAsync();
        return Result<ExpenseDto>.Success(items.First(e => e.Id == entity.Id));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _db.Expenses.FindAsync(id);
        if (entity is null) return Result.Failure("Expense not found.");
        _db.Expenses.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Delete, "Expense", id, entity.Description);
        return Result.Success();
    }
}
