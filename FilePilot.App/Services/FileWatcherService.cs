using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Timer = System.Timers.Timer;

namespace FilePilot.App.Services;

/// <summary>
/// Wraps FileSystemWatcher to detect new files in a watch folder, with debounce.
/// </summary>
public class FileWatcherService : IDisposable
{
    private FileSystemWatcher? _watcher;
    private string _watchFolder = string.Empty;
    private readonly Dictionary<string, Timer> _debounceTimers = new();
    private readonly HashSet<string> _ignoreOncePaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private const int DebounceMs = 500;

    public event Action<string>? OnFileCreated;

    public bool IsWatching { get; private set; }

    /// <summary>
    /// Ignore the next event for this file path. Used after undo to prevent
    /// the restored file from being immediately re-organized.
    /// </summary>
    public void IgnoreNext(string filePath)
    {
        lock (_lock)
        {
            _ignoreOncePaths.Add(filePath);
        }
    }

    public void Start(string watchFolder)
    {
        if (IsWatching) return;

        if (!Directory.Exists(watchFolder))
            return;

        _watchFolder = watchFolder;

        _watcher = new FileSystemWatcher(watchFolder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Error += OnError;
        IsWatching = true;
    }

    public void Stop()
    {
        if (!IsWatching || _watcher == null) return;

        _watcher.EnableRaisingEvents = false;
        _watcher.Created -= OnCreated;
        _watcher.Changed -= OnChanged;
        _watcher.Error -= OnError;
        _watcher.Dispose();
        _watcher = null;
        IsWatching = false;

        lock (_lock)
        {
            foreach (var timer in _debounceTimers.Values)
                timer.Dispose();
            _debounceTimers.Clear();
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        ScheduleDebounce(e.FullPath);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        ScheduleDebounce(e.FullPath);
    }

    private void ScheduleDebounce(string filePath)
    {
        lock (_lock)
        {
            if (_debounceTimers.TryGetValue(filePath, out var existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Start();
                return;
            }

            var timer = new Timer(DebounceMs) { AutoReset = false };
            timer.Elapsed += (_, _) =>
            {
                OnDebounceElapsed(filePath);
            };
            _debounceTimers[filePath] = timer;
            timer.Start();
        }
    }

    private void OnDebounceElapsed(string filePath)
    {
        lock (_lock)
        {
            if (_debounceTimers.TryGetValue(filePath, out var timer))
            {
                timer.Dispose();
                _debounceTimers.Remove(filePath);
            }
        }

        // Safety check: only process files directly in the watch folder.
        // macOS FSEvents may fire events for subdirectory files even with
        // IncludeSubdirectories=false, so we guard with an explicit check.
        if (!IsDirectChild(filePath))
            return;

        // Skip files that were just restored by undo.
        lock (_lock)
        {
            if (_ignoreOncePaths.Remove(filePath))
                return;
        }

        if (File.Exists(filePath))
            OnFileCreated?.Invoke(filePath);
    }

    private bool IsDirectChild(string filePath)
    {
        var parent = Path.GetDirectoryName(filePath);
        return string.Equals(parent, _watchFolder, StringComparison.OrdinalIgnoreCase);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Stop();
    }

    public void Dispose()
    {
        Stop();
    }
}
