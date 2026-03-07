using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class DeductionService : IDeductionService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    public DeductionService(AppDbContext db, IAuditService audit) { _db = db; _audit = audit; }

    public async Task<IReadOnlyList<DeductionDto>> GetAllAsync()
    {
        return await _db.Deductions
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DeductionDto
            {
                Id = d.Id, Title = d.Title, Description = d.Description,
                Amount = d.Amount, CurrencyCode = d.CurrencyCode,
                Type = d.Type, RecurrenceType = d.RecurrenceType,
                StartDate = d.StartDate, EndDate = d.EndDate,
                IsActive = d.IsActive, Notes = d.Notes, CreatedAt = d.CreatedAt
            }).ToListAsync();
    }

    public async Task<Result<DeductionDto>> CreateAsync(DeductionCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return Result<DeductionDto>.Failure("Title is required.");
        if (dto.Amount <= 0) return Result<DeductionDto>.Failure("Amount must be greater than zero.");

        var entity = new Deduction
        {
            Title = dto.Title.Trim(), Description = dto.Description?.Trim(),
            Amount = dto.Amount, CurrencyCode = dto.CurrencyCode,
            Type = dto.Type, RecurrenceType = dto.RecurrenceType,
            StartDate = dto.StartDate, EndDate = dto.EndDate,
            IsActive = dto.IsActive, Notes = dto.Notes?.Trim()
        };
        _db.Deductions.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Create, "Deduction", entity.Id, $"{entity.Title} ({entity.Amount:N2})");
        var items = await GetAllAsync();
        return Result<DeductionDto>.Success(items.First(d => d.Id == entity.Id));
    }

    public async Task<Result<DeductionDto>> UpdateAsync(DeductionUpdateDto dto)
    {
        var entity = await _db.Deductions.FindAsync(dto.Id);
        if (entity is null) return Result<DeductionDto>.Failure("Deduction not found.");
        if (string.IsNullOrWhiteSpace(dto.Title)) return Result<DeductionDto>.Failure("Title is required.");

        entity.Title = dto.Title.Trim(); entity.Description = dto.Description?.Trim();
        entity.Amount = dto.Amount; entity.CurrencyCode = dto.CurrencyCode;
        entity.Type = dto.Type; entity.RecurrenceType = dto.RecurrenceType;
        entity.StartDate = dto.StartDate; entity.EndDate = dto.EndDate;
        entity.IsActive = dto.IsActive; entity.Notes = dto.Notes?.Trim();
        await _db.SaveChangesAsync();
        var items = await GetAllAsync();
        return Result<DeductionDto>.Success(items.First(d => d.Id == entity.Id));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _db.Deductions.FindAsync(id);
        if (entity is null) return Result.Failure("Deduction not found.");
        _db.Deductions.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Delete, "Deduction", id, entity.Title);
        return Result.Success();
    }
}
