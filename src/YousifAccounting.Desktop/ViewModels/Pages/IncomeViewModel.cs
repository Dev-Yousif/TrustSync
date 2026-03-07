using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class IncomeViewModel : ViewModelBase
{
    private readonly IIncomeService _incomeService;
    private readonly ICompanyClientService _companyService;
    private readonly IProjectService _projectService;
    private List<IncomeDto> _allItems = [];

    [ObservableProperty] private ObservableCollection<IncomeDto> _items = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyClientDto> _companies = [];
    [ObservableProperty] private ObservableCollection<ProjectDto> _projects = [];

    // Editor
    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private int _editingId;
    [ObservableProperty] private string _editorDescription = string.Empty;
    [ObservableProperty] private decimal _editorAmount;
    [ObservableProperty] private string _editorCurrency = "USD";
    [ObservableProperty] private DateTimeOffset? _editorDate = DateTimeOffset.Now;
    [ObservableProperty] private IncomeSourceType _editorSourceType;
    [ObservableProperty] private PaymentStatus _editorPaymentStatus = PaymentStatus.Received;
    [ObservableProperty] private CompanyClientDto? _editorCompany;
    [ObservableProperty] private ProjectDto? _editorProject;
    [ObservableProperty] private bool _editorIsRecurring;
    [ObservableProperty] private RecurrenceType _editorRecurrenceType;
    [ObservableProperty] private string _editorNotes = string.Empty;

    // Validation
    public string? DescriptionError => GetFieldError("Description");
    public bool HasDescriptionError => HasFieldError("Description");
    public string? AmountError => GetFieldError("Amount");
    public bool HasAmountError => HasFieldError("Amount");

    public IncomeSourceType[] SourceTypes { get; } = Enum.GetValues<IncomeSourceType>();
    public PaymentStatus[] PaymentStatuses { get; } = Enum.GetValues<PaymentStatus>();
    public RecurrenceType[] RecurrenceTypes { get; } = Enum.GetValues<RecurrenceType>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public IncomeViewModel(IIncomeService incomeService, ICompanyClientService companyService, IProjectService projectService)
    {
        _incomeService = incomeService;
        _companyService = companyService;
        _projectService = projectService;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            _allItems = (await _incomeService.GetAllAsync()).ToList();
            Companies = new ObservableCollection<CompanyClientDto>(await _companyService.GetAllAsync());
            Projects = new ObservableCollection<ProjectDto>(await _projectService.GetAllAsync());
            ApplyFilter();
        }
        finally { IsBusy = false; }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText) ? _allItems
            : _allItems.Where(i => i.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
        Items = new ObservableCollection<IncomeDto>(filtered);
    }

    partial void OnEditorDescriptionChanged(string value)
        => SetFieldError("Description", string.IsNullOrWhiteSpace(value) ? "Description is required." : null);

    partial void OnEditorAmountChanged(decimal value)
        => SetFieldError("Amount", value <= 0 ? "Amount must be greater than zero." : null);

    [RelayCommand]
    private void OpenCreate()
    {
        IsEditMode = false; EditingId = 0;
        EditorDescription = string.Empty; EditorAmount = 0; EditorCurrency = "USD";
        EditorDate = DateTimeOffset.Now; EditorSourceType = IncomeSourceType.Salary;
        EditorPaymentStatus = PaymentStatus.Received; EditorCompany = null; EditorProject = null;
        EditorIsRecurring = false; EditorRecurrenceType = RecurrenceType.None; EditorNotes = string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEdit(IncomeDto item)
    {
        IsEditMode = true; EditingId = item.Id;
        EditorDescription = item.Description; EditorAmount = item.Amount; EditorCurrency = item.CurrencyCode;
        EditorDate = new DateTimeOffset(item.Date); EditorSourceType = item.SourceType;
        EditorPaymentStatus = item.PaymentStatus;
        EditorCompany = Companies.FirstOrDefault(c => c.Id == item.CompanyClientId);
        EditorProject = Projects.FirstOrDefault(p => p.Id == item.ProjectId);
        EditorIsRecurring = item.IsRecurring; EditorRecurrenceType = item.RecurrenceType;
        EditorNotes = item.Notes ?? string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand] private void CloseEditor() => IsEditorOpen = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        OnEditorDescriptionChanged(EditorDescription);
        OnEditorAmountChanged(EditorAmount);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            if (IsEditMode)
            {
                var r = await _incomeService.UpdateAsync(new IncomeUpdateDto
                {
                    Id = EditingId, Description = EditorDescription, Amount = EditorAmount,
                    CurrencyCode = EditorCurrency, Date = EditorDate?.DateTime ?? DateTime.Today, SourceType = EditorSourceType,
                    PaymentStatus = EditorPaymentStatus, CompanyClientId = EditorCompany?.Id,
                    ProjectId = EditorProject?.Id, IsRecurring = EditorIsRecurring,
                    RecurrenceType = EditorRecurrenceType, Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            else
            {
                var r = await _incomeService.CreateAsync(new IncomeCreateDto
                {
                    Description = EditorDescription, Amount = EditorAmount,
                    CurrencyCode = EditorCurrency, Date = EditorDate?.DateTime ?? DateTime.Today, SourceType = EditorSourceType,
                    PaymentStatus = EditorPaymentStatus, CompanyClientId = EditorCompany?.Id,
                    ProjectId = EditorProject?.Id, IsRecurring = EditorIsRecurring,
                    RecurrenceType = EditorRecurrenceType, Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            IsEditorOpen = false;
            await LoadAsync();
            ShowToast(IsEditMode ? "Income updated." : "Income created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDelete(IncomeDto item)
    {
        ShowConfirmDialog("Delete Income", $"Are you sure you want to delete \"{item.Description}\"?", async () =>
        {
            IsBusy = true; ClearError();
            try { var r = await _incomeService.DeleteAsync(item.Id); if (!r.IsSuccess) ErrorMessage = r.Error; else { await LoadAsync(); ShowToast("Income deleted."); } }
            finally { IsBusy = false; }
        });
    }
}
