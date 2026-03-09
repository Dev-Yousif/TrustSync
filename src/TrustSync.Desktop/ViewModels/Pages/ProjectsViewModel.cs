using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrustSync.Application.DTOs;
using TrustSync.Application.Services;
using TrustSync.Domain.Enums;
using TrustSync.Desktop.ViewModels.Base;

namespace TrustSync.Desktop.ViewModels.Pages;

public partial class ProjectsViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly ICompanyClientService _companyService;
    private List<ProjectDto> _allItems = [];

    [ObservableProperty] private ObservableCollection<ProjectDto> _items = [];
    [ObservableProperty] private ProjectDto? _selectedItem;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyClientDto> _companies = [];

    // Editor
    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private int _editingId;
    [ObservableProperty] private string _editorName = string.Empty;
    [ObservableProperty] private CompanyClientDto? _editorCompany;
    [ObservableProperty] private ProjectStatus _editorStatus = ProjectStatus.Planned;
    [ObservableProperty] private decimal _editorAgreedAmount;
    [ObservableProperty] private decimal _editorReceivedAmount;
    [ObservableProperty] private decimal _editorExpectedAmount;
    [ObservableProperty] private string _editorCurrency = "USD";
    [ObservableProperty] private DateTimeOffset? _editorStartDate;
    [ObservableProperty] private DateTimeOffset? _editorEndDate;
    [ObservableProperty] private int _editorCompletion;
    [ObservableProperty] private string _editorNotes = string.Empty;

    // Validation
    public string? NameError => GetFieldError("Name");
    public bool HasNameError => HasFieldError("Name");

    public ProjectStatus[] StatusValues { get; } = Enum.GetValues<ProjectStatus>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public ProjectsViewModel(IProjectService projectService, ICompanyClientService companyService)
    {
        _projectService = projectService;
        _companyService = companyService;
        LoadCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            _allItems = (await _projectService.GetAllAsync()).ToList();
            var companies = await _companyService.GetAllAsync();
            Companies = new ObservableCollection<CompanyClientDto>(companies);
            ApplyFilter();
        }
        finally { IsBusy = false; }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allItems
            : _allItems.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                 || (p.CompanyClientName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        Items = new ObservableCollection<ProjectDto>(filtered);
    }

    partial void OnEditorNameChanged(string value)
        => SetFieldError("Name", string.IsNullOrWhiteSpace(value) ? "Project name is required." : null);

    [RelayCommand]
    private void OpenCreate()
    {
        IsEditMode = false; EditingId = 0;
        EditorName = string.Empty; EditorCompany = null;
        EditorStatus = ProjectStatus.Planned;
        EditorAgreedAmount = 0; EditorReceivedAmount = 0; EditorExpectedAmount = 0;
        EditorCurrency = "USD"; EditorStartDate = null; EditorEndDate = null;
        EditorCompletion = 0; EditorNotes = string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEdit(ProjectDto item)
    {
        IsEditMode = true; EditingId = item.Id;
        EditorName = item.Name;
        EditorCompany = Companies.FirstOrDefault(c => c.Id == item.CompanyClientId);
        EditorStatus = item.Status;
        EditorAgreedAmount = item.AgreedAmount; EditorReceivedAmount = item.ReceivedAmount;
        EditorExpectedAmount = item.ExpectedAmount; EditorCurrency = item.CurrencyCode;
        EditorStartDate = item.StartDate.HasValue ? new DateTimeOffset(item.StartDate.Value) : null;
        EditorEndDate = item.EndDate.HasValue ? new DateTimeOffset(item.EndDate.Value) : null;
        EditorCompletion = item.CompletionPercentage;
        EditorNotes = item.Notes ?? string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand] private void CloseEditor() => IsEditorOpen = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        OnEditorNameChanged(EditorName);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            if (IsEditMode)
            {
                var result = await _projectService.UpdateAsync(new ProjectUpdateDto
                {
                    Id = EditingId, Name = EditorName, CompanyClientId = EditorCompany?.Id,
                    Status = EditorStatus, AgreedAmount = EditorAgreedAmount,
                    ReceivedAmount = EditorReceivedAmount, ExpectedAmount = EditorExpectedAmount,
                    CurrencyCode = EditorCurrency, StartDate = EditorStartDate?.DateTime,
                    EndDate = EditorEndDate?.DateTime, CompletionPercentage = EditorCompletion,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!result.IsSuccess) { ErrorMessage = result.Error; return; }
            }
            else
            {
                var result = await _projectService.CreateAsync(new ProjectCreateDto
                {
                    Name = EditorName, CompanyClientId = EditorCompany?.Id,
                    Status = EditorStatus, AgreedAmount = EditorAgreedAmount,
                    ExpectedAmount = EditorExpectedAmount, CurrencyCode = EditorCurrency,
                    StartDate = EditorStartDate?.DateTime, EndDate = EditorEndDate?.DateTime,
                    CompletionPercentage = EditorCompletion,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!result.IsSuccess) { ErrorMessage = result.Error; return; }
            }

            IsEditorOpen = false;
            await LoadAsync();
            ShowToast(IsEditMode ? "Project updated." : "Project created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDelete(ProjectDto item)
    {
        ShowConfirmDialog("Delete Project", $"Are you sure you want to delete \"{item.Name}\"?", async () =>
        {
            IsBusy = true; ClearError();
            try
            {
                var result = await _projectService.DeleteAsync(item.Id);
                if (!result.IsSuccess) { ErrorMessage = result.Error; return; }
                await LoadAsync();
                ShowToast("Project deleted.");
            }
            finally { IsBusy = false; }
        });
    }
}
