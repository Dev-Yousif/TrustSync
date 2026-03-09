using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportingService _reportingService;

    [ObservableProperty] private int _selectedYear;
    [ObservableProperty] private int _selectedMonth;
    [ObservableProperty] private int _selectedTabIndex;
    [ObservableProperty] private string _currencyCode = "USD";

    [ObservableProperty] private ObservableCollection<MonthlySummaryReport> _monthlySummaries = [];
    [ObservableProperty] private ObservableCollection<IncomeBySourceItem> _incomeBySource = [];
    [ObservableProperty] private ObservableCollection<CategoryBreakdownItem> _expenseByCategory = [];
    [ObservableProperty] private ObservableCollection<ProjectProfitabilityItem> _projectProfitability = [];

    public int[] AvailableYears { get; } = Enumerable.Range(DateTime.Now.Year - 5, 7).Reverse().ToArray();
    public int[] AvailableMonths { get; } = Enumerable.Range(1, 12).ToArray();

    public ReportsViewModel(IReportingService reportingService)
    {
        _reportingService = reportingService;
        _selectedYear = DateTime.Now.Year;
        _selectedMonth = DateTime.Now.Month;
        LoadCommand.ExecuteAsync(null);
    }

    partial void OnSelectedYearChanged(int value) => LoadCommand.ExecuteAsync(null);
    partial void OnSelectedMonthChanged(int value) => LoadCommand.ExecuteAsync(null);

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            CurrencyCode = await _reportingService.GetDefaultCurrencyCodeAsync();

            MonthlySummaries = new ObservableCollection<MonthlySummaryReport>(
                await _reportingService.GetMonthlySummariesAsync(SelectedYear));

            IncomeBySource = new ObservableCollection<IncomeBySourceItem>(
                await _reportingService.GetIncomeBySourceAsync(SelectedYear, SelectedMonth));

            ExpenseByCategory = new ObservableCollection<CategoryBreakdownItem>(
                await _reportingService.GetExpenseByCategoryAsync(SelectedYear, SelectedMonth));

            ProjectProfitability = new ObservableCollection<ProjectProfitabilityItem>(
                await _reportingService.GetProjectProfitabilityAsync());
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        try
        {
            var csv = await _reportingService.ExportMonthlySummaryToCsvAsync(SelectedYear);
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"TrustSync_Report_{SelectedYear}.csv");
            await File.WriteAllTextAsync(path, csv);
            ShowToast($"CSV exported to Desktop");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Export failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        try
        {
            var symbol = GetCurrencySymbol(CurrencyCode);
            var pdf = await _reportingService.ExportMonthlySummaryToPdfAsync(SelectedYear, symbol);
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"TrustSync_Report_{SelectedYear}.pdf");
            await File.WriteAllBytesAsync(path, pdf);
            ShowToast($"PDF exported to Desktop");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"PDF export failed: {ex.Message}";
        }
    }

    private static string GetCurrencySymbol(string code) => code switch
    {
        "USD" => "$", "EUR" => "\u20AC", "GBP" => "\u00A3", "IQD" => "IQD ",
        "AED" => "AED ", "SAR" => "SAR ", "TRY" => "\u20BA", "CAD" => "C$",
        "AUD" => "A$", "JPY" => "\u00A5", _ => code + " "
    };
}
