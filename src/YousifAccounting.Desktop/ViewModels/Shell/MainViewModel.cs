using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.Security;
using YousifAccounting.Desktop.Services;
using YousifAccounting.Desktop.ViewModels.Base;
using YousifAccounting.Desktop.ViewModels.Pages;

namespace YousifAccounting.Desktop.ViewModels.Shell;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly ISessionService _sessionService;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private string _pageTitle = "Dashboard";

    [ObservableProperty]
    private NavigationItem? _selectedNavItem;

    public IReadOnlyList<NavigationGroup> NavigationGroups { get; }

    public MainViewModel(NavigationService navigationService, ISessionService sessionService)
    {
        _navigationService = navigationService;
        _sessionService = sessionService;

        _navigationService.NavigationRequested += async (_, vm) =>
        {
            CurrentView = vm;

            // Trigger data load for dashboard when navigated to
            if (vm is DashboardViewModel dashboard)
                await dashboard.OnNavigatedToAsync();
        };

        NavigationGroups =
        [
            new NavigationGroup
            {
                Title = "MAIN",
                Items =
                [
                    new NavigationItem { Title = "Dashboard", IconKey = "IconDashboard", ViewModelType = typeof(DashboardViewModel) },
                    new NavigationItem { Title = "Income", IconKey = "IconIncome", ViewModelType = typeof(IncomeViewModel) },
                    new NavigationItem { Title = "Expenses", IconKey = "IconExpense", ViewModelType = typeof(ExpensesViewModel) },
                ]
            },
            new NavigationGroup
            {
                Title = "MANAGEMENT",
                Items =
                [
                    new NavigationItem { Title = "Companies", IconKey = "IconCompanies", ViewModelType = typeof(CompaniesViewModel) },
                    new NavigationItem { Title = "Projects", IconKey = "IconProjects", ViewModelType = typeof(ProjectsViewModel) },
                    new NavigationItem { Title = "Deductions", IconKey = "IconDeductions", ViewModelType = typeof(DeductionsViewModel) },
                    new NavigationItem { Title = "Savings", IconKey = "IconSavings", ViewModelType = typeof(SavingsViewModel) },
                ]
            },
            new NavigationGroup
            {
                Title = "INSIGHTS",
                Items =
                [
                    new NavigationItem { Title = "Reports", IconKey = "IconReports", ViewModelType = typeof(ReportsViewModel) },
                ]
            },
            new NavigationGroup
            {
                Title = "SYSTEM",
                Items =
                [
                    new NavigationItem { Title = "Backup", IconKey = "IconBackup", ViewModelType = typeof(BackupViewModel) },
                    new NavigationItem { Title = "Settings", IconKey = "IconSettings", ViewModelType = typeof(SettingsViewModel) },
                ]
            }
        ];

        // Navigate to dashboard on start
        SelectedNavItem = NavigationGroups[0].Items[0];
    }

    partial void OnSelectedNavItemChanged(NavigationItem? value)
    {
        if (value is null) return;
        NavigateToItem(value);
    }

    [RelayCommand]
    private void SelectNavItem(NavigationItem item)
    {
        if (ReferenceEquals(item, SelectedNavItem))
        {
            // Already selected — force re-navigate to get fresh data
            NavigateToItem(item);
        }
        else
        {
            SelectedNavItem = item;
        }
    }

    private void NavigateToItem(NavigationItem item)
    {
        foreach (var group in NavigationGroups)
            foreach (var navItem in group.Items)
                navItem.IsSelected = ReferenceEquals(navItem, item);

        PageTitle = item.Title;
        _navigationService.NavigateTo(item.ViewModelType);
        _sessionService.RecordActivity();
    }

    [RelayCommand]
    private void Logout()
    {
        _sessionService.EndSession();
    }
}
