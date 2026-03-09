using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Domain.Enums;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class IncomeViewModel : ViewModelBase
{
    private readonly IIncomeService _incomeService;
    private readonly ICompanyClientService _companyService;
    private readonly IProjectService _projectService;
    private readonly ICurrencyConversionService _conversionService;
    private List<IncomeDto> _allItems = [];

    [ObservableProperty] private ObservableCollection<IncomeDto> _items = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyClientDto> _companies = [];
    [ObservableProperty] private ObservableCollection<ProjectDto> _projects = [];

    // Advanced Filters
    [ObservableProperty] private bool _isFilterOpen;
    [ObservableProperty] private DateTimeOffset? _filterDateFrom;
    [ObservableProperty] private DateTimeOffset? _filterDateTo;
    [ObservableProperty] private IncomeSourceType? _filterSourceType;
    [ObservableProperty] private PaymentStatus? _filterPaymentStatus;
    [ObservableProperty] private CompanyClientDto? _filterCompany;
    [ObservableProperty] private string? _filterCurrency;
    [ObservableProperty] private decimal? _filterMinAmount;
    [ObservableProperty] private decimal? _filterMaxAmount;
    [ObservableProperty] private int _activeFilterCount;

    public IncomeSourceType?[] FilterSourceTypes { get; } =
        [null, .. Enum.GetValues<IncomeSourceType>().Cast<IncomeSourceType?>()];
    public PaymentStatus?[] FilterPaymentStatuses { get; } =
        [null, .. Enum.GetValues<PaymentStatus>().Cast<PaymentStatus?>()];
    public string?[] FilterCurrencies { get; } =
        [null, "USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

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
    [ObservableProperty] private string _conversionPreview = string.Empty;

    // Validation
    public string? DescriptionError => GetFieldError("Description");
    public bool HasDescriptionError => HasFieldError("Description");
    public string? AmountError => GetFieldError("Amount");
    public bool HasAmountError => HasFieldError("Amount");

    public IncomeSourceType[] SourceTypes { get; } = Enum.GetValues<IncomeSourceType>();
    public PaymentStatus[] PaymentStatuses { get; } = Enum.GetValues<PaymentStatus>();
    public RecurrenceType[] RecurrenceTypes { get; } = Enum.GetValues<RecurrenceType>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public IncomeViewModel(IIncomeService incomeService, ICompanyClientService companyService, IProjectService projectService, ICurrencyConversionService conversionService)
    {
        _incomeService = incomeService;
        _companyService = companyService;
        _projectService = projectService;
        _conversionService = conversionService;
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

    [RelayCommand]
    private void ToggleFilter() => IsFilterOpen = !IsFilterOpen;

    [RelayCommand]
    private void ApplyFilters()
    {
        ApplyFilter();
        IsFilterOpen = false;
    }

    [RelayCommand]
    private void ClearFilters()
    {
        FilterDateFrom = null;
        FilterDateTo = null;
        FilterSourceType = null;
        FilterPaymentStatus = null;
        FilterCompany = null;
        FilterCurrency = null;
        FilterMinAmount = null;
        FilterMaxAmount = null;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<IncomeDto> filtered = _allItems;

        // Text search
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(i => i.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || (i.Notes ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // Date range
        if (FilterDateFrom.HasValue)
            filtered = filtered.Where(i => i.Date >= FilterDateFrom.Value.Date);
        if (FilterDateTo.HasValue)
            filtered = filtered.Where(i => i.Date <= FilterDateTo.Value.Date);

        // Source type
        if (FilterSourceType.HasValue)
            filtered = filtered.Where(i => i.SourceType == FilterSourceType.Value);

        // Payment status
        if (FilterPaymentStatus.HasValue)
            filtered = filtered.Where(i => i.PaymentStatus == FilterPaymentStatus.Value);

        // Company
        if (FilterCompany is not null)
            filtered = filtered.Where(i => i.CompanyClientId == FilterCompany.Id);

        // Currency
        if (!string.IsNullOrEmpty(FilterCurrency))
            filtered = filtered.Where(i => i.CurrencyCode == FilterCurrency);

        // Amount range
        if (FilterMinAmount.HasValue)
            filtered = filtered.Where(i => i.Amount >= FilterMinAmount.Value);
        if (FilterMaxAmount.HasValue)
            filtered = filtered.Where(i => i.Amount <= FilterMaxAmount.Value);

        Items = new ObservableCollection<IncomeDto>(filtered.ToList());

        // Count active filters
        var count = 0;
        if (FilterDateFrom.HasValue) count++;
        if (FilterDateTo.HasValue) count++;
        if (FilterSourceType.HasValue) count++;
        if (FilterPaymentStatus.HasValue) count++;
        if (FilterCompany is not null) count++;
        if (!string.IsNullOrEmpty(FilterCurrency)) count++;
        if (FilterMinAmount.HasValue) count++;
        if (FilterMaxAmount.HasValue) count++;
        ActiveFilterCount = count;
    }

    partial void OnEditorDescriptionChanged(string value)
        => SetFieldError("Description", string.IsNullOrWhiteSpace(value) ? "Description is required." : null);

    partial void OnEditorAmountChanged(decimal value)
    {
        SetFieldError("Amount", value <= 0 ? "Amount must be greater than zero." : null);
        _ = UpdateConversionPreviewAsync();
    }

    partial void OnEditorCurrencyChanged(string value) => _ = UpdateConversionPreviewAsync();

    private async Task UpdateConversionPreviewAsync()
    {
        if (EditorAmount <= 0) { ConversionPreview = string.Empty; return; }
        var result = await _conversionService.ConvertToDefaultAsync(EditorAmount, EditorCurrency);
        if (result.IsSuccess && !string.Equals(EditorCurrency, result.Value!.TargetCurrencyCode, StringComparison.OrdinalIgnoreCase))
            ConversionPreview = $"\u2248 {result.Value.ConvertedAmount:N2} {result.Value.TargetCurrencyCode} (rate: {result.Value.ExchangeRateUsed:N4})";
        else
            ConversionPreview = string.Empty;
    }

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
