using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YousifAccounting.Application.Security;
using YousifAccounting.Application.Services;
using YousifAccounting.Infrastructure.Persistence;
using YousifAccounting.Infrastructure.Persistence.Seeding;
using YousifAccounting.Infrastructure.Security;
using YousifAccounting.Infrastructure.Services;

namespace YousifAccounting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string? dbPassword = null)
    {
        SQLitePCL.Batteries_V2.Init();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(DatabaseConfiguration.GetConnectionString(dbPassword));
        }, ServiceLifetime.Transient);

        services.AddTransient<DatabaseSeeder>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.AddTransient<ICompanyClientService, CompanyClientService>();
        services.AddTransient<IProjectService, ProjectService>();
        services.AddTransient<IIncomeService, IncomeService>();
        services.AddTransient<IExpenseService, ExpenseService>();
        services.AddTransient<IDeductionService, DeductionService>();
        services.AddTransient<ISavingsService, SavingsService>();
        services.AddTransient<IReportingService, ReportingService>();
        services.AddTransient<IBackupService, BackupService>();
        services.AddTransient<IAuditService, AuditService>();
        services.AddHttpClient();
        services.AddTransient<ICurrencyConversionService, CurrencyConversionService>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
