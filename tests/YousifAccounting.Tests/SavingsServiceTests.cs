using FluentAssertions;
using Xunit;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Infrastructure.Services;

namespace YousifAccounting.Tests;

public class SavingsServiceTests
{
    private SavingsService CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.Create(dbName);
        return new SavingsService(db, new NullAuditService(), new NullCurrencyConversionService());
    }

    [Fact]
    public async Task GetAllGoals_Returns_Empty_Initially()
    {
        var service = CreateService();
        var items = await service.GetAllGoalsAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateGoal_Succeeds_With_Valid_Data()
    {
        var service = CreateService();
        var dto = new SavingGoalCreateDto
        {
            Name = "Emergency Fund",
            TargetAmount = 10000m,
            CurrencyCode = "USD"
        };

        var result = await service.CreateGoalAsync(dto);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Emergency Fund");
        result.Value.TargetAmount.Should().Be(10000m);
        result.Value.SavedAmount.Should().Be(0m);
    }

    [Fact]
    public async Task CreateGoal_Fails_Without_Name()
    {
        var service = CreateService();
        var dto = new SavingGoalCreateDto { Name = "", TargetAmount = 1000m };

        var result = await service.CreateGoalAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Name");
    }

    [Fact]
    public async Task CreateGoal_Fails_With_Zero_Target()
    {
        var service = CreateService();
        var dto = new SavingGoalCreateDto { Name = "Test", TargetAmount = 0 };

        var result = await service.CreateGoalAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Target");
    }

    [Fact]
    public async Task UpdateGoal_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateGoalAsync(new SavingGoalCreateDto { Name = "Original", TargetAmount = 1000m });

        var service2 = new SavingsService(TestDbContextFactory.Create(dbName), new NullAuditService(), new NullCurrencyConversionService());
        var result = await service2.UpdateGoalAsync(new SavingGoalUpdateDto
        {
            Id = created.Value!.Id,
            Name = "Updated",
            TargetAmount = 2000m
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Updated");
        result.Value.TargetAmount.Should().Be(2000m);
    }

    [Fact]
    public async Task DeleteGoal_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var created = await service.CreateGoalAsync(new SavingGoalCreateDto { Name = "ToDelete", TargetAmount = 500m });

        var service2 = new SavingsService(TestDbContextFactory.Create(dbName), new NullAuditService(), new NullCurrencyConversionService());
        var result = await service2.DeleteGoalAsync(created.Value!.Id);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddEntry_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var goal = await service.CreateGoalAsync(new SavingGoalCreateDto { Name = "Fund", TargetAmount = 1000m });

        var service2 = new SavingsService(TestDbContextFactory.Create(dbName), new NullAuditService(), new NullCurrencyConversionService());
        var result = await service2.AddEntryAsync(new SavingEntryCreateDto
        {
            SavingGoalId = goal.Value!.Id,
            Amount = 250m,
            Date = DateTime.Today,
            Notes = "First deposit"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Amount.Should().Be(250m);
    }

    [Fact]
    public async Task AddEntry_Fails_With_Zero_Amount()
    {
        var service = CreateService();
        var result = await service.AddEntryAsync(new SavingEntryCreateDto { SavingGoalId = 1, Amount = 0 });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetEntries_Returns_Entries_For_Goal()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var goal = await service.CreateGoalAsync(new SavingGoalCreateDto { Name = "Fund", TargetAmount = 1000m });
        await service.AddEntryAsync(new SavingEntryCreateDto
        {
            SavingGoalId = goal.Value!.Id, Amount = 100m, Date = DateTime.Today
        });

        var service2 = new SavingsService(TestDbContextFactory.Create(dbName), new NullAuditService(), new NullCurrencyConversionService());
        var entries = await service2.GetEntriesAsync(goal.Value!.Id);
        entries.Should().HaveCount(1);
        entries[0].Amount.Should().Be(100m);
    }

    [Fact]
    public async Task DeleteEntry_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var service = CreateService(dbName);
        var goal = await service.CreateGoalAsync(new SavingGoalCreateDto { Name = "Fund", TargetAmount = 1000m });
        var entry = await service.AddEntryAsync(new SavingEntryCreateDto
        {
            SavingGoalId = goal.Value!.Id, Amount = 100m, Date = DateTime.Today
        });

        var service2 = new SavingsService(TestDbContextFactory.Create(dbName), new NullAuditService(), new NullCurrencyConversionService());
        var result = await service2.DeleteEntryAsync(entry.Value!.Id);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEntry_Fails_For_NonExistent()
    {
        var service = CreateService();
        var result = await service.DeleteEntryAsync(999);
        result.IsSuccess.Should().BeFalse();
    }
}
