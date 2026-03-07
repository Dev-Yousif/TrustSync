using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;

    [ObservableProperty]
    private DashboardSummaryDto _summary = new();

    [ObservableProperty]
    private ObservableCollection<MonthlyDataPoint> _monthlyTrends = [];

    [ObservableProperty]
    private ObservableCollection<CategoryBreakdownItem> _expenseByCategory = [];

    [ObservableProperty]
    private ObservableCollection<RecentTransactionDto> _recentTransactions = [];

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private int _selectedMonth;

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        var now = DateTime.Now;
        _selectedYear = now.Year;
        _selectedMonth = now.Month;
    }

    public async Task OnNavigatedToAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();

        try
        {
            var summary = await _dashboardService.GetMonthlySummaryAsync(SelectedYear, SelectedMonth);
            Summary = summary;

            var trends = await _dashboardService.GetMonthlyTrendsAsync(SelectedYear);
            MonthlyTrends = new ObservableCollection<MonthlyDataPoint>(trends);

            var categories = await _dashboardService.GetExpenseByCategoryAsync(SelectedYear, SelectedMonth);
            ExpenseByCategory = new ObservableCollection<CategoryBreakdownItem>(categories);

            var recent = await _dashboardService.GetRecentTransactionsAsync(4);
            RecentTransactions = new ObservableCollection<RecentTransactionDto>(recent);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
