using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
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

    public string UserInitial => string.IsNullOrWhiteSpace(Summary?.UserDisplayName)
        ? "" : Summary.UserDisplayName.Trim()[..1].ToUpperInvariant();

    [ObservableProperty]
    private Bitmap? _profileBitmap;

    public bool HasProfileImage => ProfileBitmap is not null;

    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Good morning,",
                < 17 => "Good afternoon,",
                _ => "Good evening,"
            };
        }
    }

    public string TodayDate => DateTime.Now.ToString("MMMM dd, yyyy");
    public string TodayDay => DateTime.Now.ToString("dddd");

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
            OnPropertyChanged(nameof(UserInitial));

            ProfileBitmap?.Dispose();
            ProfileBitmap = null;
            if (!string.IsNullOrEmpty(summary.ProfileImagePath) && File.Exists(summary.ProfileImagePath))
            {
                try { ProfileBitmap = new Bitmap(summary.ProfileImagePath); }
                catch { /* ignore */ }
            }
            OnPropertyChanged(nameof(HasProfileImage));

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
