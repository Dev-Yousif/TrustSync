using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class ExpensesViewModel : ViewModelBase
{
    private readonly IExpenseService _expenseService;
    private readonly ICompanyClientService _companyService;
    private readonly IProjectService _projectService;
    private readonly ICurrencyConversionService _conversionService;
    private List<ExpenseDto> _allItems = [];

    [ObservableProperty] private ObservableCollection<ExpenseDto> _items = [];
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<ExpenseCategoryDto> _categories = [];
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
    [ObservableProperty] private ExpenseCategoryDto? _editorCategory;
    [ObservableProperty] private ExpenseType _editorExpenseType;
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
    public string? CategoryError => GetFieldError("Category");
    public bool HasCategoryError => HasFieldError("Category");

    public ExpenseType[] ExpenseTypes { get; } = Enum.GetValues<ExpenseType>();
    public RecurrenceType[] RecurrenceTypes { get; } = Enum.GetValues<RecurrenceType>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public ExpensesViewModel(IExpenseService expenseService, ICompanyClientService companyService, IProjectService projectService, ICurrencyConversionService conversionService)
    {
        _expenseService = expenseService;
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
            _allItems = (await _expenseService.GetAllAsync()).ToList();
            Categories = new ObservableCollection<ExpenseCategoryDto>(await _expenseService.GetCategoriesAsync());
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
            : _allItems.Where(e => e.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
        Items = new ObservableCollection<ExpenseDto>(filtered);
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

    partial void OnEditorCategoryChanged(ExpenseCategoryDto? value)
        => SetFieldError("Category", value is null ? "Category is required." : null);

    [RelayCommand]
    private void OpenCreate()
    {
        IsEditMode = false; EditingId = 0;
        EditorDescription = string.Empty; EditorAmount = 0; EditorCurrency = "USD";
        EditorDate = DateTimeOffset.Now; EditorCategory = null;
        EditorExpenseType = ExpenseType.Personal; EditorCompany = null; EditorProject = null;
        EditorIsRecurring = false; EditorRecurrenceType = RecurrenceType.None; EditorNotes = string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEdit(ExpenseDto item)
    {
        IsEditMode = true; EditingId = item.Id;
        EditorDescription = item.Description; EditorAmount = item.Amount; EditorCurrency = item.CurrencyCode;
        EditorDate = new DateTimeOffset(item.Date);
        EditorCategory = Categories.FirstOrDefault(c => c.Id == item.CategoryId);
        EditorExpenseType = item.ExpenseType;
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
        OnEditorCategoryChanged(EditorCategory);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            if (IsEditMode)
            {
                var r = await _expenseService.UpdateAsync(new ExpenseUpdateDto
                {
                    Id = EditingId, Description = EditorDescription, Amount = EditorAmount,
                    CurrencyCode = EditorCurrency, Date = EditorDate?.DateTime ?? DateTime.Today, CategoryId = EditorCategory!.Id,
                    ExpenseType = EditorExpenseType, CompanyClientId = EditorCompany?.Id,
                    ProjectId = EditorProject?.Id, IsRecurring = EditorIsRecurring,
                    RecurrenceType = EditorRecurrenceType, Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            else
            {
                var r = await _expenseService.CreateAsync(new ExpenseCreateDto
                {
                    Description = EditorDescription, Amount = EditorAmount,
                    CurrencyCode = EditorCurrency, Date = EditorDate?.DateTime ?? DateTime.Today, CategoryId = EditorCategory!.Id,
                    ExpenseType = EditorExpenseType, CompanyClientId = EditorCompany?.Id,
                    ProjectId = EditorProject?.Id, IsRecurring = EditorIsRecurring,
                    RecurrenceType = EditorRecurrenceType, Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            IsEditorOpen = false;
            await LoadAsync();
            ShowToast(IsEditMode ? "Expense updated." : "Expense created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDelete(ExpenseDto item)
    {
        ShowConfirmDialog("Delete Expense", $"Are you sure you want to delete \"{item.Description}\"?", async () =>
        {
            IsBusy = true; ClearError();
            try { var r = await _expenseService.DeleteAsync(item.Id); if (!r.IsSuccess) ErrorMessage = r.Error; else { await LoadAsync(); ShowToast("Expense deleted."); } }
            finally { IsBusy = false; }
        });
    }
}
