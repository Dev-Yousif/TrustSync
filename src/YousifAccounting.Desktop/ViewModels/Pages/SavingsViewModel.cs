using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YousifAccounting.Application.DTOs;
using YousifAccounting.Application.Services;
using YousifAccounting.Desktop.ViewModels.Base;

namespace YousifAccounting.Desktop.ViewModels.Pages;

public partial class SavingsViewModel : ViewModelBase
{
    private readonly ISavingsService _service;

    [ObservableProperty] private ObservableCollection<SavingGoalDto> _goals = [];
    [ObservableProperty] private SavingGoalDto? _selectedGoal;
    [ObservableProperty] private ObservableCollection<SavingEntryDto> _entries = [];

    // Goal editor
    [ObservableProperty] private bool _isGoalEditorOpen;
    [ObservableProperty] private bool _isGoalEditMode;
    [ObservableProperty] private int _editingGoalId;
    [ObservableProperty] private string _editorGoalName = string.Empty;
    [ObservableProperty] private string _editorGoalDescription = string.Empty;
    [ObservableProperty] private decimal _editorTargetAmount;
    [ObservableProperty] private string _editorCurrency = "USD";
    [ObservableProperty] private DateTimeOffset? _editorTargetDate;

    // Entry editor
    [ObservableProperty] private bool _isEntryEditorOpen;
    [ObservableProperty] private decimal _editorEntryAmount;
    [ObservableProperty] private DateTimeOffset _editorEntryDate = DateTimeOffset.Now;
    [ObservableProperty] private string _editorEntryNotes = string.Empty;

    // Validation
    public string? GoalNameError => GetFieldError("GoalName");
    public bool HasGoalNameError => HasFieldError("GoalName");
    public string? TargetAmountError => GetFieldError("TargetAmount");
    public bool HasTargetAmountError => HasFieldError("TargetAmount");
    public string? EntryAmountError => GetFieldError("EntryAmount");
    public bool HasEntryAmountError => HasFieldError("EntryAmount");

    public string[] Currencies { get; } = ["USD", "EUR", "GBP", "IQD", "AED", "SAR", "TRY", "CAD", "AUD", "JPY"];

    public SavingsViewModel(ISavingsService service)
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
            Goals = new ObservableCollection<SavingGoalDto>(await _service.GetAllGoalsAsync());
            if (SelectedGoal is not null)
            {
                var goal = Goals.FirstOrDefault(g => g.Id == SelectedGoal.Id);
                SelectedGoal = goal;
                if (goal is not null)
                    Entries = new ObservableCollection<SavingEntryDto>(await _service.GetEntriesAsync(goal.Id));
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void SelectGoal(SavingGoalDto goal)
    {
        SelectedGoal = goal;
    }

    partial void OnSelectedGoalChanged(SavingGoalDto? value)
    {
        if (value is not null)
            LoadEntriesCommand.ExecuteAsync(null);
        else
            Entries.Clear();
    }

    [RelayCommand]
    private async Task LoadEntriesAsync()
    {
        if (SelectedGoal is null) return;
        Entries = new ObservableCollection<SavingEntryDto>(await _service.GetEntriesAsync(SelectedGoal.Id));
    }

    partial void OnEditorGoalNameChanged(string value)
        => SetFieldError("GoalName", string.IsNullOrWhiteSpace(value) ? "Name is required." : null);

    partial void OnEditorTargetAmountChanged(decimal value)
        => SetFieldError("TargetAmount", value <= 0 ? "Target amount must be greater than zero." : null);

    partial void OnEditorEntryAmountChanged(decimal value)
        => SetFieldError("EntryAmount", value <= 0 ? "Amount must be greater than zero." : null);

    [RelayCommand]
    private void OpenCreateGoal()
    {
        IsGoalEditMode = false; EditingGoalId = 0;
        EditorGoalName = string.Empty; EditorGoalDescription = string.Empty;
        EditorTargetAmount = 0; EditorCurrency = "USD"; EditorTargetDate = null;
        IsGoalEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand]
    private void OpenEditGoal(SavingGoalDto goal)
    {
        IsGoalEditMode = true; EditingGoalId = goal.Id;
        EditorGoalName = goal.Name; EditorGoalDescription = goal.Description ?? string.Empty;
        EditorTargetAmount = goal.TargetAmount; EditorCurrency = goal.CurrencyCode;
        EditorTargetDate = goal.TargetDate.HasValue ? new DateTimeOffset(goal.TargetDate.Value) : null;
        IsGoalEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand] private void CloseGoalEditor() => IsGoalEditorOpen = false;

    [RelayCommand]
    private async Task SaveGoalAsync()
    {
        OnEditorGoalNameChanged(EditorGoalName);
        OnEditorTargetAmountChanged(EditorTargetAmount);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            if (IsGoalEditMode)
            {
                var r = await _service.UpdateGoalAsync(new SavingGoalUpdateDto
                {
                    Id = EditingGoalId, Name = EditorGoalName,
                    Description = string.IsNullOrWhiteSpace(EditorGoalDescription) ? null : EditorGoalDescription,
                    TargetAmount = EditorTargetAmount, CurrencyCode = EditorCurrency,
                    TargetDate = EditorTargetDate?.DateTime
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            else
            {
                var r = await _service.CreateGoalAsync(new SavingGoalCreateDto
                {
                    Name = EditorGoalName,
                    Description = string.IsNullOrWhiteSpace(EditorGoalDescription) ? null : EditorGoalDescription,
                    TargetAmount = EditorTargetAmount, CurrencyCode = EditorCurrency,
                    TargetDate = EditorTargetDate?.DateTime
                });
                if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            }
            IsGoalEditorOpen = false;
            await LoadAsync();
            ShowToast(IsGoalEditMode ? "Saving goal updated." : "Saving goal created.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDeleteGoal(SavingGoalDto goal)
    {
        ShowConfirmDialog("Delete Saving Goal", $"Are you sure you want to delete \"{goal.Name}\" and all its entries?", async () =>
        {
            IsBusy = true; ClearError();
            try { var r = await _service.DeleteGoalAsync(goal.Id); if (!r.IsSuccess) ErrorMessage = r.Error; else { SelectedGoal = null; await LoadAsync(); ShowToast("Saving goal deleted."); } }
            finally { IsBusy = false; }
        });
    }

    [RelayCommand]
    private void OpenAddEntry()
    {
        if (SelectedGoal is null) return;
        EditorEntryAmount = 0; EditorEntryDate = DateTimeOffset.Now; EditorEntryNotes = string.Empty;
        IsEntryEditorOpen = true; ClearError(); ClearAllFieldErrors();
    }

    [RelayCommand] private void CloseEntryEditor() => IsEntryEditorOpen = false;

    [RelayCommand]
    private async Task SaveEntryAsync()
    {
        if (SelectedGoal is null) return;
        OnEditorEntryAmountChanged(EditorEntryAmount);
        if (HasAnyValidationError()) return;

        IsBusy = true; ClearError();
        try
        {
            var r = await _service.AddEntryAsync(new SavingEntryCreateDto
            {
                SavingGoalId = SelectedGoal.Id, Amount = EditorEntryAmount,
                Date = EditorEntryDate.DateTime,
                Notes = string.IsNullOrWhiteSpace(EditorEntryNotes) ? null : EditorEntryNotes
            });
            if (!r.IsSuccess) { ErrorMessage = r.Error; return; }
            IsEntryEditorOpen = false;
            await LoadAsync();
            ShowToast("Entry added.");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void RequestDeleteEntry(SavingEntryDto entry)
    {
        ShowConfirmDialog("Delete Entry", $"Are you sure you want to delete this ${entry.Amount:N2} entry?", async () =>
        {
            IsBusy = true; ClearError();
            try { var r = await _service.DeleteEntryAsync(entry.Id); if (!r.IsSuccess) ErrorMessage = r.Error; else { await LoadAsync(); ShowToast("Entry deleted."); } }
            finally { IsBusy = false; }
        });
    }
}
