using FluentAssertions;
using Xunit;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Services;

namespace YousifAccounting.Tests;

public class DeductionServiceTests
{
    private DeductionService CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.Create(dbName);
        return new DeductionService(db, new NullAuditService());
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
        var dto = new DeductionCreateDto
        {
            Title = "Health Insurance",
            Amount = 350m,
            CurrencyCode = "USD",
            Type = DeductionType.Recurring,
            RecurrenceType = RecurrenceType.Monthly,
            StartDate = DateTime.Today,
            IsActive = true
        };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Health Insurance");
        result.Value.Amount.Should().Be(350m);
    }

    [Fact]
    public async Task Create_Fails_Without_Title()
    {
        var service = CreateService();
        var dto = new DeductionCreateDto { Title = "", Amount = 100m };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Title");
    }

    [Fact]
    public async Task Create_Fails_With_Zero_Amount()
    {
        var service = CreateService();
        var dto = new DeductionCreateDto { Title = "Test", Amount = 0 };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Amount");
    }

    [Fact]
    public async Task Update_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new DeductionCreateDto { Title = "Original", Amount = 100m });

        var service2 = new DeductionService(TestDbContextFactory.Create(dbName), new NullAuditService());
        var result = await service2.UpdateAsync(new DeductionUpdateDto
        {
            Id = created.Value!.Id,
            Title = "Updated",
            Amount = 200m
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new DeductionCreateDto { Title = "ToDelete", Amount = 50m });

        var service2 = new DeductionService(TestDbContextFactory.Create(dbName), new NullAuditService());
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
