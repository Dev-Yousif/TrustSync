using FluentAssertions;
using Xunit;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Services;

namespace YousifAccounting.Tests;

public class IncomeServiceTests
{
    private IncomeService CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.Create(dbName);
        return new IncomeService(db, new NullAuditService());
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
        var dto = new IncomeCreateDto
        {
            Description = "Freelance Payment",
            Amount = 1500m,
            CurrencyCode = "USD",
            Date = DateTime.Today,
            SourceType = IncomeSourceType.Freelance,
            PaymentStatus = PaymentStatus.Received
        };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Freelance Payment");
        result.Value.Amount.Should().Be(1500m);
    }

    [Fact]
    public async Task Create_Fails_Without_Description()
    {
        var service = CreateService();
        var dto = new IncomeCreateDto { Description = "", Amount = 100m };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Description");
    }

    [Fact]
    public async Task Create_Fails_With_Zero_Amount()
    {
        var service = CreateService();
        var dto = new IncomeCreateDto { Description = "Test", Amount = 0 };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Amount");
    }

    [Fact]
    public async Task Create_Trims_Description()
    {
        var service = CreateService();
        var dto = new IncomeCreateDto { Description = "  Padded  ", Amount = 100m };

        var result = await service.CreateAsync(dto);
        result.Value!.Description.Should().Be("Padded");
    }

    [Fact]
    public async Task Update_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new IncomeCreateDto { Description = "Original", Amount = 100m });

        var service2 = new IncomeService(TestDbContextFactory.Create(dbName), new NullAuditService());
        var result = await service2.UpdateAsync(new IncomeUpdateDto
        {
            Id = created.Value!.Id,
            Description = "Updated",
            Amount = 200m
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Updated");
        result.Value.Amount.Should().Be(200m);
    }

    [Fact]
    public async Task Update_Fails_For_NonExistent()
    {
        var service = CreateService();
        var result = await service.UpdateAsync(new IncomeUpdateDto { Id = 999, Description = "Test", Amount = 100m });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Delete_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateAsync(new IncomeCreateDto { Description = "ToDelete", Amount = 100m });

        var service2 = new IncomeService(TestDbContextFactory.Create(dbName), new NullAuditService());
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
