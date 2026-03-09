using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Desktop.ViewModels.Base;
using TrustSync.Domain.Enums;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IExpenseService _expenseService;
    private readonly ICurrencyConversionService _conversionService;

    [ObservableProperty]
    private DashboardSummaryDto _summary = new();

    [ObservableProperty]
    private ObservableCollection<CategoryBreakdownItem> _expenseByCategory = [];

    [ObservableProperty]
    private ObservableCollection<RecentTransactionDto> _recentTransactions = [];

    [ObservableProperty]
    private ObservableCollection<DashboardSavingGoalDto> _savingGoals = [];

    [ObservableProperty]
    private int _selectedYear;

    [ObservableProperty]
    private int _selectedMonth;

    // Spending Insights
    [ObservableProperty] private string _insightExpenseChange = string.Empty;
    [ObservableProperty] private string _insightIncomeChange = string.Empty;
    [ObservableProperty] private string _insightTopCategory = string.Empty;
    [ObservableProperty] private string _insightTransactions = string.Empty;
    [ObservableProperty] private bool _expenseChangeIsUp;
    [ObservableProperty] private bool _incomeChangeIsUp;

    // Quick Expense
    [ObservableProperty] private bool _isQuickExpenseOpen;
    [ObservableProperty] private string _quickExpenseDescription = string.Empty;
    [ObservableProperty] private decimal _quickExpenseAmount;
    [ObservableProperty] private string _quickExpenseCurrency = "USD";
    [ObservableProperty] private ExpenseCategoryDto? _quickExpenseCategory;
    [ObservableProperty] private ObservableCollection<ExpenseCategoryDto> _expenseCategories = [];
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

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

    public string SelectedMonthLabel => new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");

    public bool IsCurrentMonth => SelectedYear == DateTime.Now.Year && SelectedMonth == DateTime.Now.Month;

    public DashboardViewModel(IDashboardService dashboardService, IExpenseService expenseService, ICurrencyConversionService conversionService)
    {
        _dashboardService = dashboardService;
        _expenseService = expenseService;
        _conversionService = conversionService;

        var now = DateTime.Now;
        _selectedYear = now.Year;
        _selectedMonth = now.Month;
    }

    public async Task OnNavigatedToAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        if (SelectedMonth == 1)
        {
            SelectedMonth = 12;
            SelectedYear--;
        }
        else
        {
            SelectedMonth--;
        }
        OnPropertyChanged(nameof(SelectedMonthLabel));
        OnPropertyChanged(nameof(IsCurrentMonth));
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        var now = DateTime.Now;
        if (SelectedYear >= now.Year && SelectedMonth >= now.Month) return;

        if (SelectedMonth == 12)
        {
            SelectedMonth = 1;
            SelectedYear++;
        }
        else
        {
            SelectedMonth++;
        }
        OnPropertyChanged(nameof(SelectedMonthLabel));
        OnPropertyChanged(nameof(IsCurrentMonth));
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToCurrentMonthAsync()
    {
        var now = DateTime.Now;
        SelectedYear = now.Year;
        SelectedMonth = now.Month;
        OnPropertyChanged(nameof(SelectedMonthLabel));
        OnPropertyChanged(nameof(IsCurrentMonth));
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

            BuildInsights(summary);

            var categories = await _dashboardService.GetExpenseByCategoryAsync(SelectedYear, SelectedMonth);
            ExpenseByCategory = new ObservableCollection<CategoryBreakdownItem>(categories);

            var recent = await _dashboardService.GetRecentTransactionsAsync(4);
            RecentTransactions = new ObservableCollection<RecentTransactionDto>(recent);

            var goals = await _dashboardService.GetSavingGoalsSummaryAsync();
            SavingGoals = new ObservableCollection<DashboardSavingGoalDto>(goals);

            var cats = await _expenseService.GetCategoriesAsync();
            ExpenseCategories = new ObservableCollection<ExpenseCategoryDto>(cats);
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

    private void BuildInsights(DashboardSummaryDto s)
    {
        if (s.PreviousMonthExpenses > 0)
        {
            var pct = (s.MonthlyExpenses - s.PreviousMonthExpenses) / s.PreviousMonthExpenses * 100;
            ExpenseChangeIsUp = pct > 0;
            InsightExpenseChange = pct > 0
                ? $"Expenses up {pct:F0}% vs last month"
                : pct < 0
                    ? $"Expenses down {Math.Abs(pct):F0}% vs last month"
                    : "Expenses same as last month";
        }
        else
        {
            ExpenseChangeIsUp = false;
            InsightExpenseChange = s.MonthlyExpenses > 0 ? "First month tracking expenses" : "No expenses yet";
        }

        if (s.PreviousMonthIncome > 0)
        {
            var pct = (s.MonthlyIncome - s.PreviousMonthIncome) / s.PreviousMonthIncome * 100;
            IncomeChangeIsUp = pct >= 0;
            InsightIncomeChange = pct > 0
                ? $"Income up {pct:F0}% vs last month"
                : pct < 0
                    ? $"Income down {Math.Abs(pct):F0}% vs last month"
                    : "Income same as last month";
        }
        else
        {
            IncomeChangeIsUp = true;
            InsightIncomeChange = s.MonthlyIncome > 0 ? "First month tracking income" : "No income yet";
        }

        InsightTopCategory = !string.IsNullOrEmpty(s.TopExpenseCategory)
            ? $"Biggest spend: {s.TopExpenseCategory} ({s.TopExpenseCategoryAmount:N2})"
            : "No expenses recorded";

        InsightTransactions = $"{s.TransactionCount} transactions this month";
    }

    [RelayCommand]
    private void OpenQuickExpense()
    {
        QuickExpenseDescription = string.Empty;
        QuickExpenseAmount = 0;
        QuickExpenseCurrency = Summary.CurrencyCode;
        QuickExpenseCategory = null;
        IsQuickExpenseOpen = true;
        ClearError();
    }

    [RelayCommand]
    private void CloseQuickExpense() => IsQuickExpenseOpen = false;

    [RelayCommand]
    private async Task SaveQuickExpenseAsync()
    {
        if (string.IsNullOrWhiteSpace(QuickExpenseDescription))
        {
            ErrorMessage = "Description is required.";
            return;
        }
        if (QuickExpenseAmount <= 0)
        {
            ErrorMessage = "Amount must be greater than zero.";
            return;
        }
        if (QuickExpenseCategory is null)
        {
            ErrorMessage = "Please select a category.";
            return;
        }

        IsBusy = true;
        ClearError();
        try
        {
            var result = await _expenseService.CreateAsync(new ExpenseCreateDto
            {
                Description = QuickExpenseDescription,
                Amount = QuickExpenseAmount,
                CurrencyCode = QuickExpenseCurrency,
                Date = DateTime.Today,
                CategoryId = QuickExpenseCategory.Id,
                ExpenseType = ExpenseType.Personal
            });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error;
                return;
            }

            IsQuickExpenseOpen = false;
            ShowToast("Expense added!");
            await LoadDataAsync();
        }
        finally { IsBusy = false; }
    }
}
