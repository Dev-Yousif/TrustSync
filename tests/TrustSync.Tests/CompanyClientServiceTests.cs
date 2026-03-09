using FluentAssertions;
using Xunit;
using TrustSync.Application.DTOs;
using TrustSync.Domain.Enums;
using TrustSync.Infrastructure.Services;

namespace TrustSync.Tests;

public class CompanyClientServiceTests
{
    private CompanyClientService CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.Create(dbName);
        return new CompanyClientService(db, new NullAuditService());
    }

    [Fact]
    public async Task GetAll_Returns_Empty_Initially()
    {
        var service = CreateService();
        var items = await service.GetAllAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_Succeeds_With_Valid_Data()
    {
        var service = CreateService();
        var dto = new CompanyClientCreateDto
        {
            Name = "Acme Corp",
            Type = CompanyType.Company,
            EngagementType = EngagementType.Freelance,
            Status = CompanyStatus.Active,
            DefaultCurrencyCode = "USD"
        };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task Create_Fails_Without_Name()
    {
        var service = CreateService();
        var dto = new CompanyClientCreateDto { Name = "" };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public async Task Create_Trims_Name()
    {
        var service = CreateService();
        var dto = new CompanyClientCreateDto { Name = "  Acme Corp  " };

        var result = await service.CreateAsync(dto);
        result.Value!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetById_Returns_Correct_Entity()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new CompanyClientCreateDto { Name = "Test Co" });

        var service2 = new CompanyClientService(TestDbContextFactory.Create(dbName), new NullAuditService());
        var result = await service2.GetByIdAsync(created.Value!.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Co");
    }

    [Fact]
    public async Task GetById_Returns_Null_For_NonExistent()
    {
        var service = CreateService();
        var result = await service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new CompanyClientCreateDto { Name = "Original" });

        var service2 = new CompanyClientService(TestDbContextFactory.Create(dbName), new NullAuditService());
        var result = await service2.UpdateAsync(new CompanyClientUpdateDto
        {
            Id = created.Value!.Id,
            Name = "Updated",
            Status = CompanyStatus.Inactive
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_Fails_For_NonExistent()
    {
        var service = CreateService();
        var result = await service.UpdateAsync(new CompanyClientUpdateDto { Id = 999, Name = "Test" });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new CompanyClientCreateDto { Name = "ToDelete" });

        var service2 = new CompanyClientService(TestDbContextFactory.Create(dbName), new NullAuditService());
        var result = await service2.DeleteAsync(created.Value!.Id);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Fails_For_NonExistent()
    {
        var service = CreateService();
        var result = await service.DeleteAsync(999);
        result.IsSuccess.Should().BeFalse();
    }
}
