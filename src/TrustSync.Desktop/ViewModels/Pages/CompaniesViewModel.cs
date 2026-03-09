using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Domain.Enums;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class CompaniesViewModel : ViewModelBase
{
    private readonly ICompanyClientService _service;
    private List<CompanyClientDto> _allItems = [];

    [ObservableProperty]
    private ObservableCollection<CompanyClientDto> _items = [];

    [ObservableProperty]
    private CompanyClientDto? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Editor fields
    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _editingId;

    [ObservableProperty]
    private string _editorName = string.Empty;

    [ObservableProperty]
    private CompanyType _editorType;

    [ObservableProperty]
    private EngagementType _editorEngagementType;

    [ObservableProperty]
    private CompanyStatus _editorStatus = CompanyStatus.Active;

    [ObservableProperty]
    private string _editorContactEmail = string.Empty;

    [ObservableProperty]
    private string _editorNotes = string.Empty;

    [ObservableProperty]
    private string _editorCurrency = "USD";

    // Validation
    public string? NameError => GetFieldError("Name");
    public bool HasNameError => HasFieldError("Name");

    public CompanyType[] CompanyTypes { get; } = Enum.GetValues<CompanyType>();
    public EngagementType[] EngagementTypes { get; } = Enum.GetValues<EngagementType>();
    public CompanyStatus[] StatusValues { get; } = Enum.GetValues<CompanyStatus>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public CompaniesViewModel(ICompanyClientService service)
    {
        _service = service;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            _allItems = (await _service.GetAllAsync()).ToList();
            ApplyFilter();
        }
        finally { IsBusy = false; }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allItems
            : _allItems.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                 || (c.ContactEmail?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        Items = new ObservableCollection<CompanyClientDto>(filtered);
    }

    partial void OnEditorNameChanged(string value)
        => SetFieldError("Name", string.IsNullOrWhiteSpace(value) ? "Name is required." : null);

    [RelayCommand]
    private void OpenCreate()
    {
        IsEditMode = false;
        EditingId = 0;
        EditorName = string.Empty;
        EditorType = CompanyType.Company;
        EditorEngagementType = EngagementType.Freelance;
        EditorStatus = CompanyStatus.Active;
        EditorContactEmail = string.Empty;
        EditorNotes = string.Empty;
        EditorCurrency = "USD";
        IsEditorOpen = true;
        ClearError();
        ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEdit(CompanyClientDto item)
    {
        IsEditMode = true;
        EditingId = item.Id;
        EditorName = item.Name;
        EditorType = item.Type;
        EditorEngagementType = item.EngagementType;
        EditorStatus = item.Status;
        EditorContactEmail = item.ContactEmail ?? string.Empty;
        EditorNotes = item.Notes ?? string.Empty;
        EditorCurrency = item.DefaultCurrencyCode;
        IsEditorOpen = true;
        ClearError();
        ClearAllFieldErrors();
    }

    [RelayCommand]
    private void CloseEditor()
    {
        IsEditorOpen = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        OnEditorNameChanged(EditorName);
        if (HasAnyValidationError()) return;

        IsBusy = true;
        ClearError();
        try
        {
            if (IsEditMode)
            {
                var result = await _service.UpdateAsync(new CompanyClientUpdateDto
                {
                    Id = EditingId,
                    Name = EditorName,
                    Type = EditorType,
                    EngagementType = EditorEngagementType,
                    Status = EditorStatus,
                    ContactEmail = string.IsNullOrWhiteSpace(EditorContactEmail) ? null : EditorContactEmail,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes,
                    DefaultCurrencyCode = EditorCurrency
                });
                if (!result.IsSuccess) { ErrorMessage = result.Error; return; }
            }
            else
            {
                var result = await _service.CreateAsync(new CompanyClientCreateDto
                {
                    Name = EditorName,
                    Type = EditorType,
                    EngagementType = EditorEngagementType,
                    Status = EditorStatus,
                    ContactEmail = string.IsNullOrWhiteSpace(EditorContactEmail) ? null : EditorContactEmail,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes,
                    DefaultCurrencyCode = EditorCurrency
                });
                if (!result.IsSuccess) { ErrorMessage = result.Error; return; }
            }

            IsEditorOpen = false;
            await LoadAsync();
            ShowToast(IsEditMode ? "Company updated." : "Company created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDelete(CompanyClientDto item)
    {
        ShowConfirmDialog("Delete Company", $"Are you sure you want to delete \"{item.Name}\"?", async () =>
        {
            IsBusy = true;
            ClearError();
            try
            {
                var result = await _service.DeleteAsync(item.Id);
                if (!result.IsSuccess)
                {
                    ErrorMessage = result.Error;
                    return;
                }
                await LoadAsync();
                ShowToast("Company deleted.");
            }
            finally { IsBusy = false; }
        });
    }
}
