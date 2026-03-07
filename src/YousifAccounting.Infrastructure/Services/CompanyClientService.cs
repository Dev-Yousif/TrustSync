using Microsoft.EntityFrameworkCore;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Entities;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Persistence;

namespace YousifAccounting.Infrastructure.Services;

public class CompanyClientService : ICompanyClientService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public CompanyClientService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<CompanyClientDto>> GetAllAsync()
    {
        return await _db.CompanyClients
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CompanyClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                EngagementType = c.EngagementType,
                Status = c.Status,
                ContactEmail = c.ContactEmail,
                Notes = c.Notes,
                DefaultCurrencyCode = c.DefaultCurrencyCode,
                ProjectsCount = c.Projects.Count,
                TotalIncome = (decimal)c.Incomes.Sum(i => (double)i.Amount),
                TotalExpenses = (decimal)c.Expenses.Sum(e => (double)e.Amount),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CompanyClientDto?> GetByIdAsync(int id)
    {
        return await _db.CompanyClients
            .Where(c => c.Id == id)
            .Select(c => new CompanyClientDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                EngagementType = c.EngagementType,
                Status = c.Status,
                ContactEmail = c.ContactEmail,
                Notes = c.Notes,
                DefaultCurrencyCode = c.DefaultCurrencyCode,
                ProjectsCount = c.Projects.Count,
                TotalIncome = (decimal)c.Incomes.Sum(i => (double)i.Amount),
                TotalExpenses = (decimal)c.Expenses.Sum(e => (double)e.Amount),
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Result<CompanyClientDto>> CreateAsync(CompanyClientCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<CompanyClientDto>.Failure("Company name is required.");

        var entity = new CompanyClient
        {
            Name = dto.Name.Trim(),
            Type = dto.Type,
            EngagementType = dto.EngagementType,
            Status = dto.Status,
            ContactEmail = dto.ContactEmail?.Trim(),
            Notes = dto.Notes?.Trim(),
            DefaultCurrencyCode = dto.DefaultCurrencyCode
        };

        _db.CompanyClients.Add(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Create, "CompanyClient", entity.Id, entity.Name);

        var result = await GetByIdAsync(entity.Id);
        return Result<CompanyClientDto>.Success(result!);
    }

    public async Task<Result<CompanyClientDto>> UpdateAsync(CompanyClientUpdateDto dto)
    {
        var entity = await _db.CompanyClients.FindAsync(dto.Id);
        if (entity is null)
            return Result<CompanyClientDto>.Failure("Company not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<CompanyClientDto>.Failure("Company name is required.");

        entity.Name = dto.Name.Trim();
        entity.Type = dto.Type;
        entity.EngagementType = dto.EngagementType;
        entity.Status = dto.Status;
        entity.ContactEmail = dto.ContactEmail?.Trim();
        entity.Notes = dto.Notes?.Trim();
        entity.DefaultCurrencyCode = dto.DefaultCurrencyCode;

        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(entity.Id);
        return Result<CompanyClientDto>.Success(result!);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _db.CompanyClients.FindAsync(id);
        if (entity is null)
            return Result.Failure("Company not found.");

        _db.CompanyClients.Remove(entity);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(AuditAction.Delete, "CompanyClient", id, entity.Name);
        return Result.Success();
    }
}
