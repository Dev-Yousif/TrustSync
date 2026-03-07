using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Domain.Enums;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class DeductionsViewModel : ViewModelBase
{
    private readonly IDeductionService _service;

    [ObservableProperty] private ObservableCollection<DeductionDto> _items = [];
    [ObservableProperty] private string _searchText = string.Empty;

    // Editor
    [ObservableProperty] private bool _isEditorOpen;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private int _editingId;
    [ObservableProperty] private string _editorTitle = string.Empty;
    [ObservableProperty] private string _editorDescription = string.Empty;
    [ObservableProperty] private decimal _editorAmount;
    [ObservableProperty] private string _editorCurrency = "USD";
    [ObservableProperty] private DeductionType _editorType;
    [ObservableProperty] private RecurrenceType _editorRecurrenceType = RecurrenceType.Monthly;
    [ObservableProperty] private DateTimeOffset _editorStartDate = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset? _editorEndDate;
    [ObservableProperty] private bool _editorIsActive = true;
    [ObservableProperty] private string _editorNotes = string.Empty;

    // Validation
    public string? TitleError => GetFieldError("Title");
    public bool HasTitleError => HasFieldError("Title");
    public string? AmountError => GetFieldError("Amount");
    public bool HasAmountError => HasFieldError("Amount");

    public DeductionType[] DeductionTypes { get; } = Enum.GetValues<DeductionType>();
    public RecurrenceType[] RecurrenceTypes { get; } = Enum.GetValues<RecurrenceType>();
    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public DeductionsViewModel(IDeductionService service)
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
            var all = await _service.GetAllAsync();
            Items = new ObservableCollection<DeductionDto>(
                string.IsNullOrWhiteSpace(SearchText) ? all
                : all.Where(d => d.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }
        finally { IsBusy = false; }
    }

    partial void OnSearchTextChanged(string value) => LoadCommand.ExecuteAsync(null);

    partial void OnEditorTitleChanged(string value)
        => SetFieldError("Title", string.IsNullOrWhiteSpace(value) ? "Title is required." : null);

    partial void OnEditorAmountChanged(decimal value)
        => SetFieldError("Amount", value <= 0 ? "Amount must be greater than zero." : null);

    [RelayCommand]
    private void OpenCreate()
    {
        IsEditMode = false; EditingId = 0;
        EditorTitle = string.Empty; EditorDescription = string.Empty; EditorAmount = 0;
        EditorCurrency = "USD"; EditorType = DeductionType.Recurring;
        EditorRecurrenceType = RecurrenceType.Monthly;
        EditorStartDate = DateTimeOffset.Now; EditorEndDate = null;
        EditorIsActive = true; EditorNotes = string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEdit(DeductionDto item)
    {
        IsEditMode = true; EditingId = item.Id;
        EditorTitle = item.Title; EditorDescription = item.Description ?? string.Empty;
        EditorAmount = item.Amount; EditorCurrency = item.CurrencyCode;
        EditorType = item.Type; EditorRecurrenceType = item.RecurrenceType;
        EditorStartDate = new DateTimeOffset(item.StartDate);
        EditorEndDate = item.EndDate.HasValue ? new DateTimeOffset(item.EndDate.Value) : null;
        EditorIsActive = item.IsActive; EditorNotes = item.Notes ?? string.Empty;
        IsEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand] private void CloseEditor() => IsEditorOpen = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        OnEditorTitleChanged(EditorTitle);
        OnEditorAmountChanged(EditorAmount);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            if (IsEditMode)
            {
                var r = await _service.UpdateAsync(new DeductionUpdateDto
                {
                    Id = EditingId, Title = EditorTitle, Description = string.IsNullOrWhiteSpace(EditorDescription) ? null : EditorDescription,
                    Amount = EditorAmount, CurrencyCode = EditorCurrency, Type = EditorType,
                    RecurrenceType = EditorRecurrenceType, StartDate = EditorStartDate.DateTime,
                    EndDate = EditorEndDate?.DateTime, IsActive = EditorIsActive,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            else
            {
                var r = await _service.CreateAsync(new DeductionCreateDto
                {
                    Title = EditorTitle, Description = string.IsNullOrWhiteSpace(EditorDescription) ? null : EditorDescription,
                    Amount = EditorAmount, CurrencyCode = EditorCurrency, Type = EditorType,
                    RecurrenceType = EditorRecurrenceType, StartDate = EditorStartDate.DateTime,
                    EndDate = EditorEndDate?.DateTime, IsActive = EditorIsActive,
                    Notes = string.IsNullOrWhiteSpace(EditorNotes) ? null : EditorNotes
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            IsEditorOpen = false;
            await LoadAsync();
            ShowToast(IsEditMode ? "Deduction updated." : "Deduction created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDelete(DeductionDto item)
    {
        ShowConfirmDialog("Delete Deduction", $"Are you sure you want to delete \"{item.Title}\"?", async () =>
        {
            IsBusy = true; ClearError();
            try { var r = await _service.DeleteAsync(item.Id); if (!r.IsSuccess) ErrorMessage = r.Error; else { await LoadAsync(); ShowToast("Deduction deleted."); } }
            finally { IsBusy = false; }
        });
    }
}
