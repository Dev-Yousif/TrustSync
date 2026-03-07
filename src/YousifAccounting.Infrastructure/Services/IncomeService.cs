using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class IncomeService : IIncomeService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrencyConversionService _conversion;

    public IncomeService(AppDbContext db, IAuditService audit, ICurrencyConversionService conversion) { _db = db; _audit = audit; _conversion = conversion; }

    public async Task<IReadOnlyList<IncomeDto>> GetAllAsync()
    {
        return await _db.Incomes
            .Include(i => i.CompanyClient)
            .Include(i => i.Project)
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.CreatedAt)
            .Select(i => new IncomeDto
            {
                Id = i.Id,
                Description = i.Description,
                Amount = i.Amount,
                CurrencyCode = i.CurrencyCode,
                Date = i.Date,
                SourceType = i.SourceType,
                PaymentStatus = i.PaymentStatus,
                CompanyClientId = i.CompanyClientId,
                CompanyClientName = i.CompanyClient != null ? i.CompanyClient.Name : null,
                ProjectId = i.ProjectId,
                ProjectName = i.Project != null ? i.Project.Name : null,
                IsRecurring = i.IsRecurring,
                RecurrenceType = i.RecurrenceType,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<Result<IncomeDto>> CreateAsync(IncomeCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result<IncomeDto>.Failure("Description is required.");
        if (dto.Amount <= 0)
            return Result<IncomeDto>.Failure("Amount must be greater than zero.");

        var entity = new Income
        {
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            CurrencyCode = dto.CurrencyCode,
            Date = dto.Date,
            SourceType = dto.SourceType,
            PaymentStatus = dto.PaymentStatus,
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

        _db.Incomes.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Create, "Income", entity.Id, $"{entity.Description} ({entity.Amount:N2} {entity.CurrencyCode})");

        var items = await GetAllAsync();
        return Result<IncomeDto>.Success(items.First(i => i.Id == entity.Id));
    }

    public async Task<Result<IncomeDto>> UpdateAsync(IncomeUpdateDto dto)
    {
        var entity = await _db.Incomes.FindAsync(dto.Id);
        if (entity is null) return Result<IncomeDto>.Failure("Income not found.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            return Result<IncomeDto>.Failure("Description is required.");
        if (dto.Amount <= 0)
            return Result<IncomeDto>.Failure("Amount must be greater than zero.");

        entity.Description = dto.Description.Trim();
        entity.Amount = dto.Amount;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.Date = dto.Date;
        entity.SourceType = dto.SourceType;
        entity.PaymentStatus = dto.PaymentStatus;
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
        return Result<IncomeDto>.Success(items.First(i => i.Id == entity.Id));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _db.Incomes.FindAsync(id);
        if (entity is null) return Result.Failure("Income not found.");
        _db.Incomes.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Delete, "Income", id, entity.Description);
        return Result.Success();
    }
}
