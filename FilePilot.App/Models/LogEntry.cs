using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FilePilot.App.Services;

namespace FilePilot.App.Models;

public partial class LogEntry : ObservableObject
{
    public string SourcePath { get; set; } = string.Empty;
    public string DestPath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public string ActionLabel { get; set; } = Loc.LogLabelSuccess;

    public bool IsInfo { get; set; }

    /// <summary>Marks entries whose undo has already been consumed.</summary>
    public bool HasBeenUndone { get; set; }

    /// <summary>Only successful, unconsumed file moves can be undone.</summary>
    public bool IsUndoable => Success && !IsInfo && ActionLabel == Loc.LogLabelSuccess && !HasBeenUndone;

    /// <summary>Whether selection circles are visible (set by ViewModel).</summary>
    [ObservableProperty]
    private bool _canSelectForUndo;

    /// <summary>Toggled by user clicking the selection circle.</summary>
    [ObservableProperty]
    private bool _isSelectedForUndo;

    public string SelectionCircleBg => IsSelectedForUndo ? "#22C55E" : "Transparent";

    public string SelectionCircleBorder => IsSelectedForUndo ? "#22C55E" : "#9CA3AF";

    public string FileName => System.IO.Path.GetFileName(SourcePath);
    public string DisplayText => IsInfo ? ActionLabel : $"{ActionLabel}  {FileName}";
    public string StatusColor => IsInfo ? "#9CA3AF" : (Success ? "#22C55E" : "#EF4444");
    public string TimeDisplay => Timestamp.ToString("HH:mm:ss");

    public LogEntry() { }

    public LogEntry(string sourcePath, string destPath, bool success = true, string? error = null)
    {
        SourcePath = sourcePath;
        DestPath = destPath;
        Timestamp = DateTime.Now;
        Success = success;
        ErrorMessage = error;
        ActionLabel = success ? Loc.LogLabelSuccess : Loc.LogLabelSkipped;
    }

    public static LogEntry Info(string message)
    {
        return new LogEntry
        {
            ActionLabel = message,
            IsInfo = true,
            Timestamp = DateTime.Now
        };
    }

    partial void OnIsSelectedForUndoChanged(bool value)
    {
        OnPropertyChanged(nameof(SelectionCircleBg));
        OnPropertyChanged(nameof(SelectionCircleBorder));
    }
}
