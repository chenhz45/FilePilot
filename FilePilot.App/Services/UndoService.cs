using System;
using System.Collections.Generic;
using System.IO;
using FilePilot.App.Models;

namespace FilePilot.App.Services;

/// <summary>
/// Tracks file move operations and supports undoing them in reverse order.
/// </summary>
public class UndoService
{
    private readonly Stack<LogEntry> _undoStack = new();

    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Record a successful move for potential undo.
    /// </summary>
    public void Record(LogEntry entry)
    {
        if (entry.Success && !string.IsNullOrEmpty(entry.DestPath))
        {
            _undoStack.Push(entry);
        }
    }

    /// <summary>
    /// Undo the most recent move. Returns null if stack is empty.
    /// </summary>
    public LogEntry? UndoLast()
    {
        if (_undoStack.Count == 0)
            return null;

        var entry = _undoStack.Pop();
        var result = UndoEntry(entry);

        // If undo failed, the entry is re-pushed inside UndoEntry
        if (result.Success)
        {
            return result;
        }
        else
        {
            // UndoEntry already re-pushed on failure
            return result;
        }
    }

    /// <summary>
    /// Undo a set of specific entries. Pops all, undoes selected ones,
    /// and re-pushes unselected entries preserving original order.
    /// </summary>
    public List<LogEntry> UndoMultiple(HashSet<LogEntry> selectedEntries)
    {
        var results = new List<LogEntry>();
        var keepList = new List<LogEntry>();

        while (_undoStack.Count > 0)
        {
            var entry = _undoStack.Pop();
            if (selectedEntries.Contains(entry))
            {
                var result = UndoEntry(entry);
                results.Add(result);
            }
            else
            {
                keepList.Add(entry);
            }
        }

        // Re-push unselected entries in reverse order to preserve original stack order
        for (int i = keepList.Count - 1; i >= 0; i--)
        {
            _undoStack.Push(keepList[i]);
        }

        return results;
    }

    /// <summary>
    /// Core undo logic for a single entry. Does NOT pop from stack.
    /// On failure, re-pushes the entry back onto the stack.
    /// </summary>
    private LogEntry UndoEntry(LogEntry entry)
    {
        try
        {
            if (!File.Exists(entry.DestPath))
            {
                return new LogEntry(entry.DestPath, entry.SourcePath, false,
                    "文件已不存在，无法撤销")
                {
                    ActionLabel = "撤回失败"
                };
            }

            var sourceDir = Path.GetDirectoryName(entry.SourcePath);
            if (!string.IsNullOrEmpty(sourceDir) && !Directory.Exists(sourceDir))
            {
                Directory.CreateDirectory(sourceDir);
            }

            var destPath = ConflictResolver.GetUniqueFilePath(
                Path.GetDirectoryName(entry.SourcePath)!,
                Path.GetFileName(entry.SourcePath));

            File.Move(entry.DestPath, destPath);

            entry.HasBeenUndone = true;

            return new LogEntry(entry.DestPath, destPath, true)
            {
                ActionLabel = "成功撤回"
            };
        }
        catch (Exception ex)
        {
            _undoStack.Push(entry);
            return new LogEntry(entry.DestPath, entry.SourcePath, false, ex.Message)
            {
                ActionLabel = "撤回失败"
            };
        }
    }
}
