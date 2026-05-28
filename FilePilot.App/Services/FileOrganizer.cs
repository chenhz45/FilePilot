using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FilePilot.App.Models;

namespace FilePilot.App.Services;

/// <summary>
/// Core file organization logic: scan, classify, and move files based on rules.
/// </summary>
public class FileOrganizer
{
    private readonly ConfigService _configService;
    private readonly UndoService _undoService;

    /// <summary>
    /// Fired for each file operation so the UI can display progress.
    /// </summary>
    public event Action<LogEntry>? OnFileProcessed;

    public FileOrganizer(ConfigService configService, UndoService undoService)
    {
        _configService = configService;
        _undoService = undoService;
    }

    /// <summary>
    /// Main entry point: scan the source folder and organize all files into the destination folder.
    /// </summary>
    public List<LogEntry> Organize()
    {
        var config = _configService.LoadConfig();
        var logEntries = new List<LogEntry>();

        if (!Directory.Exists(config.SourceFolder))
        {
            logEntries.Add(new LogEntry(config.SourceFolder, "", false, Loc.SourceFolderNotExist));
            return logEntries;
        }

        var files = ScanFiles(config.SourceFolder, config.IgnoredFolders);

        foreach (var filePath in files)
        {
            if (config.ArchiveEnabled)
            {
                var archiveEntry = TryArchive(filePath, config);
                if (archiveEntry != null)
                {
                    logEntries.Add(archiveEntry);
                    if (archiveEntry.Success) _undoService.Record(archiveEntry);
                    OnFileProcessed?.Invoke(archiveEntry);
                    continue;
                }
            }

            var entry = ClassifyAndMove(filePath, config.Rules, config.DestinationFolder);
            logEntries.Add(entry);
            if (entry.Success) _undoService.Record(entry);
            OnFileProcessed?.Invoke(entry);
        }

        return logEntries;
    }

    /// <summary>
    /// Organize a single file (used by FileWatcher for real-time mode).
    /// </summary>
    public LogEntry OrganizeSingleFile(string filePath)
    {
        var config = _configService.LoadConfig();

        // Defense-in-depth: only process files directly in the source folder.
        var parent = Path.GetDirectoryName(filePath);
        if (!string.Equals(parent, config.SourceFolder, StringComparison.OrdinalIgnoreCase))
        {
            return new LogEntry(filePath, "", false, Loc.FileNotInRoot);
        }

        if (config.ArchiveEnabled)
        {
            var entry = TryArchive(filePath, config);
            if (entry != null)
            {
                if (entry.Success) _undoService.Record(entry);
                return entry;
            }
        }

        var result = ClassifyAndMove(filePath, config.Rules, config.DestinationFolder);
        if (result.Success) _undoService.Record(result);
        return result;
    }

    private List<string> ScanFiles(string sourceFolder, List<string> ignoredFolders)
    {
        var files = new List<string>();

        try
        {
            foreach (var filePath in Directory.EnumerateFiles(sourceFolder))
            {
                files.Add(filePath);
            }
        }
        catch (UnauthorizedAccessException) { }

        return files;
    }

    private LogEntry ClassifyAndMove(string filePath, List<Rule> rules, string destinationBase)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        var matchedRule = rules.FirstOrDefault(r =>
            r.Enabled &&
            string.Equals(r.Extension, extension, StringComparison.OrdinalIgnoreCase));

        if (matchedRule == null)
        {
            return new LogEntry(filePath, "", false, Loc.NoMatchingRule(extension));
        }

        var destDir = Path.Combine(destinationBase, matchedRule.TargetFolder);
        return MoveFile(filePath, destDir);
    }

    private LogEntry? TryArchive(string filePath, AppConfig config)
    {
        var lastWrite = File.GetLastWriteTime(filePath);
        var age = DateTime.Now - lastWrite;

        if (age.TotalDays <= config.ArchiveDays)
            return null;

        var monthDir = lastWrite.ToString("yyyy-MM");
        var archiveDir = Path.Combine(config.DestinationFolder, Loc.ArchiveDir, monthDir);

        return MoveFile(filePath, archiveDir);
    }

    private LogEntry MoveFile(string sourcePath, string destDir)
    {
        try
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var fileName = Path.GetFileName(sourcePath);
            var destPath = ConflictResolver.GetUniqueFilePath(destDir, fileName);

            File.Move(sourcePath, destPath);

            return new LogEntry(sourcePath, destPath, true);
        }
        catch (Exception ex)
        {
            return new LogEntry(sourcePath, "", false, ex.Message);
        }
    }
}
