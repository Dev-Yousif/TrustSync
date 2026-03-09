using Microsoft.EntityFrameworkCore;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Domain.Common;
using TrustSync.Domain.Entities;
using TrustSync.Infrastructure.Persistence;

namespace TrustSync.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync()
    {
        return await _db.Projects
            .Include(p => p.CompanyClient)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                CompanyClientId = p.CompanyClientId,
                CompanyClientName = p.CompanyClient != null ? p.CompanyClient.Name : null,
                Status = p.Status,
                AgreedAmount = p.AgreedAmount,
                ReceivedAmount = p.ReceivedAmount,
                ExpectedAmount = p.ExpectedAmount,
                CurrencyCode = p.CurrencyCode,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CompletionPercentage = p.CompletionPercentage,
                Notes = p.Notes,
                TotalExpenses = (decimal)p.Expenses.Sum(e => (double)e.Amount),
                Tags = p.ProjectTags.Select(pt => pt.Tag.Name).ToList(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        return await _db.Projects
            .Include(p => p.CompanyClient)
            .Include(p => p.ProjectTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.Id == id)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                CompanyClientId = p.CompanyClientId,
                CompanyClientName = p.CompanyClient != null ? p.CompanyClient.Name : null,
                Status = p.Status,
                AgreedAmount = p.AgreedAmount,
                ReceivedAmount = p.ReceivedAmount,
                ExpectedAmount = p.ExpectedAmount,
                CurrencyCode = p.CurrencyCode,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CompletionPercentage = p.CompletionPercentage,
                Notes = p.Notes,
                TotalExpenses = (decimal)p.Expenses.Sum(e => (double)e.Amount),
                Tags = p.ProjectTags.Select(pt => pt.Tag.Name).ToList(),
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Result<ProjectDto>> CreateAsync(ProjectCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<ProjectDto>.Failure("Project name is required.");

        var entity = new Project
        {
            Name = dto.Name.Trim(),
            CompanyClientId = dto.CompanyClientId,
            Status = dto.Status,
            AgreedAmount = dto.AgreedAmount,
            ExpectedAmount = dto.ExpectedAmount,
            CurrencyCode = dto.CurrencyCode,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CompletionPercentage = dto.CompletionPercentage,
            Notes = dto.Notes?.Trim()
        };

        _db.Projects.Add(entity);
        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(entity.Id);
        return Result<ProjectDto>.Success(result!);
    }

    public async Task<Result<ProjectDto>> UpdateAsync(ProjectUpdateDto dto)
    {
        var entity = await _db.Projects.FindAsync(dto.Id);
        if (entity is null)
            return Result<ProjectDto>.Failure("Project not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<ProjectDto>.Failure("Project name is required.");

        entity.Name = dto.Name.Trim();
        entity.CompanyClientId = dto.CompanyClientId;
        entity.Status = dto.Status;
        entity.AgreedAmount = dto.AgreedAmount;
        entity.ReceivedAmount = dto.ReceivedAmount;
        entity.ExpectedAmount = dto.ExpectedAmount;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.CompletionPercentage = dto.CompletionPercentage;
        entity.Notes = dto.Notes?.Trim();

        await _db.SaveChangesAsync();

        var result = await GetByIdAsync(entity.Id);
        return Result<ProjectDto>.Success(result!);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _db.Projects.FindAsync(id);
        if (entity is null)
            return Result.Failure("Project not found.");

        _db.Projects.Remove(entity);
        await _db.SaveChangesAsync();
        return Result.Success();
    }
}
