using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FilePilot.App.Models;
using FilePilot.App.Services;

namespace FilePilot.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ConfigService _configService = new();
    private readonly UndoService _undoService = new();
    private readonly FileOrganizer _fileOrganizer;
    private readonly FileWatcherService _fileWatcher = new();

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _fileCountText = "";

    [ObservableProperty]
    private string _manualButtonBg = "#9CA3AF";

    [ObservableProperty]
    private string _autoButtonBg = "#9CA3AF";

    [ObservableProperty]
    private string _newExtension = string.Empty;

    [ObservableProperty]
    private string _newTargetFolder = string.Empty;

    [ObservableProperty]
    private bool _isOrganizing;

    [ObservableProperty]
    private bool _isConfirmVisible;

    [ObservableProperty]
    private string _confirmMessage = string.Empty;

    [ObservableProperty]
    private bool _isMultiUndoMode;

    [ObservableProperty]
    private string _sourceFolderPath = string.Empty;

    [ObservableProperty]
    private string _destinationFolderPath = string.Empty;

    public bool IsNotMultiUndoMode => !IsMultiUndoMode;

    [ObservableProperty]
    private bool _isModeSelected;

    private bool _isRealTimeMode;
    private TaskCompletionSource<bool>? _confirmTcs;

    public bool IsNotOrganizing => !IsOrganizing;
    public string OrganizeButtonText => IsOrganizing ? Loc.Organizing : Loc.Organize;
    public bool HasNoRules => Rules.Count == 0;
    public bool HasNoLogs => LogEntries.Count == 0;
    public bool CanOrganize => IsNotOrganizing && IsModeSelected;

    partial void OnIsOrganizingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotOrganizing));
        OnPropertyChanged(nameof(OrganizeButtonText));
        OnPropertyChanged(nameof(CanOrganize));
    }

    partial void OnIsModeSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanOrganize));
    }

    public ObservableCollection<Rule> Rules { get; } = new();
    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public MainWindowViewModel()
    {
        _fileOrganizer = new FileOrganizer(_configService, _undoService);
        _fileOrganizer.OnFileProcessed += OnFileProcessed;
        _fileWatcher.OnFileCreated += OnWatcherFileCreated;

        var config = _configService.LoadConfig();
        SourceFolderPath = config.SourceFolder;
        DestinationFolderPath = string.Empty;
        config.DestinationFolder = string.Empty;
        _configService.SaveConfig(config);
        LoadRules();
        RefreshEmptyStates();
        UpdateIdleStatus();
    }

    partial void OnIsMultiUndoModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotMultiUndoMode));
    }

    // ── Rules ──

    private void LoadRules()
    {
        Rules.Clear();
        var config = _configService.LoadConfig();
        foreach (var rule in config.Rules)
        {
            rule.PropertyChanged += (_, _) => SaveRules();
            Rules.Add(rule);
        }

        StatusText = Rules.Count == 0 ? Loc.NoRules : Loc.RulesLoaded(Rules.Count);
        RefreshEmptyStates();
    }

    private void RefreshEmptyStates()
    {
        OnPropertyChanged(nameof(HasNoRules));
        OnPropertyChanged(nameof(HasNoLogs));
    }

    private void UpdateIdleStatus()
    {
        if (!IsOrganizing)
        {
            if (string.IsNullOrWhiteSpace(SourceFolderPath) || string.IsNullOrWhiteSpace(DestinationFolderPath))
                StatusText = Loc.SelectFoldersFirst;
            else if (!IsModeSelected)
                StatusText = Loc.SelectModeFirst;
            else
                StatusText = Loc.RulesLoaded(Rules.Count);
        }
    }

    private void SortRules()
    {
        var sorted = Rules.OrderByDescending(r => r.Enabled).ToList();
        Rules.Clear();
        foreach (var rule in sorted)
            Rules.Add(rule);
    }

    public void SaveRules()
    {
        var config = _configService.LoadConfig();
        config.Rules = Rules.ToList();
        _configService.SaveConfig(config);
        SortRules();
    }

    [RelayCommand]
    private void AddRule()
    {
        var ext = NewExtension?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(ext))
        {
            StatusText = Loc.ExtensionRequired;
            return;
        }

        if (string.IsNullOrWhiteSpace(NewTargetFolder))
        {
            StatusText = Loc.TargetFolderRequired;
            return;
        }

        if (!ext.StartsWith('.'))
            ext = $".{ext}";

        if (Rules.Any(r => r.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase)))
        {
            StatusText = Loc.RuleExists(ext);
            return;
        }

        var folder = NewTargetFolder;
        var rule = new Rule(ext, folder, enabled: true);
        rule.PropertyChanged += (_, _) => SaveRules();
        Rules.Add(rule);
        SaveRules();

        NewExtension = string.Empty;
        NewTargetFolder = string.Empty;
        StatusText = Loc.RuleAdded(ext, folder);
        RefreshEmptyStates();
    }

    public void DeleteRule(Rule rule)
    {
        if (rule == null) return;

        Rules.Remove(rule);
        SaveRules();
        StatusText = Loc.RuleDeleted(rule.Extension, rule.TargetFolder);
        RefreshEmptyStates();
    }

    // ── Folder Management ──

    public void ChangeSourceFolder(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath)) return;
        if (!Directory.Exists(newPath)) return;

        var config = _configService.LoadConfig();
        config.SourceFolder = newPath;
        _configService.SaveConfig(config);

        SourceFolderPath = newPath;

        if (_isRealTimeMode)
        {
            _fileWatcher.Stop();
            _fileWatcher.Start(newPath);
        }

        UpdateIdleStatus();
    }

    public void ChangeDestinationFolder(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath)) return;
        if (!Directory.Exists(newPath)) return;

        var config = _configService.LoadConfig();
        config.DestinationFolder = newPath;
        _configService.SaveConfig(config);

        DestinationFolderPath = newPath;
        UpdateIdleStatus();
    }

    // ── Mode ──

    private void SetModeButtons(bool isRealTime)
    {
        ManualButtonBg = isRealTime ? "#9CA3AF" : "#22C55E";
        AutoButtonBg = isRealTime ? "#22C55E" : "#9CA3AF";
    }

    [RelayCommand]
    private void SelectManual()
    {
        if (!_isRealTimeMode && ManualButtonBg == "#22C55E") return;

        _isRealTimeMode = false;
        _fileWatcher.Stop();
        SetModeButtons(false);
        IsModeSelected = true;

        var config = _configService.LoadConfig();
        config.Mode = OrganizeMode.Manual;
        _configService.SaveConfig(config);

        LogEntries.Insert(0, LogEntry.Info(Loc.ManualModeLog));
        UpdateIdleStatus();
    }

    [RelayCommand]
    private async Task SelectAuto()
    {
        if (_isRealTimeMode) return;

        _confirmTcs = new TaskCompletionSource<bool>();
        ConfirmMessage = Loc.AutoModeConfirmMsg;
        IsConfirmVisible = true;

        var confirmed = await _confirmTcs.Task;
        if (!confirmed)
            return;

        _isRealTimeMode = true;
        var config = _configService.LoadConfig();
        _fileWatcher.Start(config.SourceFolder);
        SetModeButtons(true);
        IsModeSelected = true;

        config.Mode = OrganizeMode.RealTime;
        _configService.SaveConfig(config);

        LogEntries.Insert(0, LogEntry.Info(Loc.AutoModeLog));
        StatusText = Loc.SwitchingToAuto;
        await Organize();
    }

    [RelayCommand]
    private void Confirm()
    {
        IsConfirmVisible = false;
        _confirmTcs?.TrySetResult(true);
    }

    [RelayCommand]
    private void CancelConfirm()
    {
        IsConfirmVisible = false;
        _confirmTcs?.TrySetResult(false);
    }

    private void OnWatcherFileCreated(string filePath)
    {
        var entry = _fileOrganizer.OrganizeSingleFile(filePath);
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LogEntries.Insert(0, entry);
            StatusText = entry.Success
                ? Loc.AutoOrganized(entry.FileName)
                : Loc.FileSkipped(entry.FileName, entry.ErrorMessage);

            FileCountText = Loc.UndoCount(_undoService.UndoCount);
        });
    }

    // ── Undo: single ──

    [RelayCommand]
    private void UndoLast()
    {
        if (_undoService.UndoCount == 0)
        {
            StatusText = Loc.NoUndoAvailable;
            return;
        }

        var result = _undoService.UndoLast();

        if (result == null)
        {
            StatusText = Loc.UndoFailed;
            return;
        }

        LogEntries.Insert(0, result);

        if (result.Success)
        {
            _fileWatcher.IgnoreNext(result.DestPath);
            StatusText = Loc.UndoLastSuccess;
            _ = NotificationService.ShowAsync(Loc.NotifUndoTitle, Loc.NotifUndoLastBody);
        }
        else
        {
            StatusText = Loc.UndoFailedWithError(result.ErrorMessage);
        }

        FileCountText = Loc.UndoCount(_undoService.UndoCount);
        RefreshEmptyStates();
    }

    // ── Undo: multi ──

    [RelayCommand]
    private void EnterMultiUndoMode()
    {
        IsMultiUndoMode = true;
        foreach (var entry in LogEntries)
            entry.CanSelectForUndo = entry.IsUndoable;
        StatusText = Loc.SelectUndoOps;
    }

    [RelayCommand]
    private void CancelMultiUndo()
    {
        ExitMultiUndoMode();
        StatusText = Loc.MultiUndoCancelled;
    }

    public void ToggleUndoSelection(LogEntry entry)
    {
        if (!entry.CanSelectForUndo) return;
        entry.IsSelectedForUndo = !entry.IsSelectedForUndo;
    }

    [RelayCommand]
    private void SelectAllUndoable()
    {
        foreach (var entry in LogEntries)
        {
            if (entry.CanSelectForUndo)
                entry.IsSelectedForUndo = true;
        }
    }

    [RelayCommand]
    private void ExecuteMultiUndo()
    {
        var selected = LogEntries.Where(e => e.IsSelectedForUndo).ToHashSet();
        if (selected.Count == 0)
        {
            StatusText = Loc.NoUndoSelected;
            return;
        }

        var results = _undoService.UndoMultiple(selected);

        int successCount = 0;
        foreach (var result in results)
        {
            LogEntries.Insert(0, result);
            if (result.Success)
            {
                _fileWatcher.IgnoreNext(result.DestPath);
                successCount++;
            }
        }

        ExitMultiUndoMode();
        StatusText = Loc.UndoMultipleSuccess(successCount);
        FileCountText = Loc.UndoCount(_undoService.UndoCount);

        if (successCount > 0)
            _ = NotificationService.ShowAsync(Loc.NotifUndoTitle, Loc.NotifUndoMultiBody(successCount));

        RefreshEmptyStates();
    }

    private void ExitMultiUndoMode()
    {
        IsMultiUndoMode = false;
        foreach (var entry in LogEntries)
        {
            entry.CanSelectForUndo = false;
            entry.IsSelectedForUndo = false;
        }
    }

    // ── Organize ──

    [RelayCommand]
    private async Task Organize()
    {
        if (IsOrganizing) return;

        IsOrganizing = true;
        StatusText = Loc.ScanningFiles;
        FileCountText = "";

        try
        {
            var results = await Task.Run(() => _fileOrganizer.Organize());

            var successCount = results.Count(r => r.Success);
            var failCount = results.Count(r => !r.Success);

            StatusText = successCount > 0 || failCount > 0
                ? Loc.OrganizeDone(successCount, failCount, _undoService.UndoCount)
                : Loc.NoFilesToOrganize;

            FileCountText = Loc.UndoCount(_undoService.UndoCount);

            if (successCount > 0 || failCount > 0)
                _ = NotificationService.ShowAsync(Loc.NotifOrganizeTitle,
                    Loc.NotifOrganizeBody(successCount, failCount));

            RefreshEmptyStates();
        }
        catch (Exception ex)
        {
            StatusText = Loc.OrganizeError(ex.Message);
        }
        finally
        {
            IsOrganizing = false;
        }
    }

    private void OnFileProcessed(LogEntry entry)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            LogEntries.Insert(0, entry);
            StatusText = entry.Success
                ? Loc.FileMoved(entry.FileName)
                : Loc.FileSkipped(entry.FileName, entry.ErrorMessage);
        });
    }
}
