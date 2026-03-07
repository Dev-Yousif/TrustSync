using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class SavingsService : ISavingsService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    public SavingsService(AppDbContext db, IAuditService audit) { _db = db; _audit = audit; }

    public async Task<IReadOnlyList<SavingGoalDto>> GetAllGoalsAsync()
    {
        return await _db.SavingGoals
            .Include(g => g.Entries)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new SavingGoalDto
            {
                Id = g.Id, Name = g.Name, Description = g.Description,
                TargetAmount = g.TargetAmount,
                SavedAmount = (decimal)g.Entries.Sum(e => (double)e.Amount),
                ProgressPercentage = g.TargetAmount > 0
                    ? g.Entries.Sum(e => (double)e.Amount) / (double)g.TargetAmount * 100
                    : 0,
                CurrencyCode = g.CurrencyCode,
                TargetDate = g.TargetDate,
                IsCompleted = g.IsCompleted,
                CreatedAt = g.CreatedAt
            }).ToListAsync();
    }

    public async Task<Result<SavingGoalDto>> CreateGoalAsync(SavingGoalCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return Result<SavingGoalDto>.Failure("Name is required.");
        if (dto.TargetAmount <= 0) return Result<SavingGoalDto>.Failure("Target amount must be greater than zero.");

        var entity = new SavingGoal
        {
            Name = dto.Name.Trim(), Description = dto.Description?.Trim(),
            TargetAmount = dto.TargetAmount, CurrencyCode = dto.CurrencyCode,
            TargetDate = dto.TargetDate
        };
        _db.SavingGoals.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Create, "SavingGoal", entity.Id, entity.Name);
        var items = await GetAllGoalsAsync();
        return Result<SavingGoalDto>.Success(items.First(g => g.Id == entity.Id));
    }

    public async Task<Result<SavingGoalDto>> UpdateGoalAsync(SavingGoalUpdateDto dto)
    {
        var entity = await _db.SavingGoals.FindAsync(dto.Id);
        if (entity is null) return Result<SavingGoalDto>.Failure("Goal not found.");

        entity.Name = dto.Name.Trim(); entity.Description = dto.Description?.Trim();
        entity.TargetAmount = dto.TargetAmount; entity.CurrencyCode = dto.CurrencyCode;
        entity.TargetDate = dto.TargetDate; entity.IsCompleted = dto.IsCompleted;
        await _db.SaveChangesAsync();
        var items = await GetAllGoalsAsync();
        return Result<SavingGoalDto>.Success(items.First(g => g.Id == entity.Id));
    }

    public async Task<Result> DeleteGoalAsync(int id)
    {
        var entity = await _db.SavingGoals.FindAsync(id);
        if (entity is null) return Result.Failure("Goal not found.");
        _db.SavingGoals.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Delete, "SavingGoal", id, entity.Name);
        return Result.Success();
    }

    public async Task<IReadOnlyList<SavingEntryDto>> GetEntriesAsync(int goalId)
    {
        return await _db.SavingEntries
            .Where(e => e.SavingGoalId == goalId)
            .OrderByDescending(e => e.Date)
            .Select(e => new SavingEntryDto
            {
                Id = e.Id, SavingGoalId = e.SavingGoalId,
                Amount = e.Amount, Date = e.Date, Notes = e.Notes
            }).ToListAsync();
    }

    public async Task<Result<SavingEntryDto>> AddEntryAsync(SavingEntryCreateDto dto)
    {
        if (dto.Amount <= 0) return Result<SavingEntryDto>.Failure("Amount must be greater than zero.");

        var entity = new SavingEntry
        {
            SavingGoalId = dto.SavingGoalId, Amount = dto.Amount,
            Date = dto.Date, Notes = dto.Notes?.Trim()
        };
        _db.SavingEntries.Add(entity);
        await _db.SaveChangesAsync();
        return Result<SavingEntryDto>.Success(new SavingEntryDto
        {
            Id = entity.Id, SavingGoalId = entity.SavingGoalId,
            Amount = entity.Amount, Date = entity.Date, Notes = entity.Notes
        });
    }

    public async Task<Result> DeleteEntryAsync(int id)
    {
        var entity = await _db.SavingEntries.FindAsync(id);
        if (entity is null) return Result.Failure("Entry not found.");
        _db.SavingEntries.Remove(entity);
        await _db.SaveChangesAsync();
        return Result.Success();
    }
}
