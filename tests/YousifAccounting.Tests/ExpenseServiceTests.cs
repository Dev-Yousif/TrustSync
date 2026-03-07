using FluentAssertions;
using Xunit;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Infrastructure.Services;

namespace YousifAccounting.Tests;

public class ExpenseServiceTests
{
    private (ExpenseService service, int categoryId) CreateService(string? dbName = null)
    {
        var db = TestDbContextFactory.CreateWithCategory(out var categoryId, dbName);
        return (new ExpenseService(db, new NullAuditService()), categoryId);
    }

    [Fact]
    public async Task GetAll_Returns_Empty_Initially()
    {
        var (service, _) = CreateService();
        var items = await service.GetAllAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_Succeeds_With_Valid_Data()
    {
        var (service, categoryId) = CreateService();
        var dto = new ExpenseCreateDto
        {
            Description = "Groceries",
            Amount = 85.50m,
            CurrencyCode = "USD",
            Date = DateTime.Today,
            CategoryId = categoryId,
            ExpenseType = ExpenseType.Personal
        };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().Be("Groceries");
        result.Value.Amount.Should().Be(85.50m);
    }

    [Fact]
    public async Task Create_Fails_Without_Description()
    {
        var (service, categoryId) = CreateService();
        var dto = new ExpenseCreateDto { Description = "", Amount = 100m, CategoryId = categoryId };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Description");
    }

    [Fact]
    public async Task Create_Fails_With_Zero_Amount()
    {
        var (service, categoryId) = CreateService();
        var dto = new ExpenseCreateDto { Description = "Test", Amount = 0, CategoryId = categoryId };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Amount");
    }

    [Fact]
    public async Task Create_Fails_Without_Category()
    {
        var (service, _) = CreateService();
        var dto = new ExpenseCreateDto { Description = "Test", Amount = 100m, CategoryId = 0 };

        var result = await service.CreateAsync(dto);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Category");
    }

    [Fact]
    public async Task Delete_Succeeds()
    {
        var dbName = Guid.NewGuid().ToString();
        var (service, categoryId) = CreateService(dbName);
        var created = await service.CreateAsync(new ExpenseCreateDto
        {
            Description = "ToDelete", Amount = 50m, CategoryId = categoryId
        });

        var db2 = TestDbContextFactory.Create(dbName);
        var service2 = new ExpenseService(db2, new NullAuditService());
        var result = await service2.DeleteAsync(created.Value!.Id);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_Fails_For_NonExistent()
    {
        var (service, _) = CreateService();
        var result = await service.DeleteAsync(999);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategories_Returns_Seeded_Category()
    {
        var (service, _) = CreateService();
        var categories = await service.GetCategoriesAsync();
        categories.Should().NotBeEmpty();
        categories.Should().Contain(c => c.Name == "Food");
    }
}
