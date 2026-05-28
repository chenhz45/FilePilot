using System;
using System.Collections.Generic;

namespace FilePilot.App.Models;

public enum OrganizeMode
{
    Manual,
    RealTime
}

/// <summary>
/// Application configuration, persisted to ~/.filepilot/config.json
/// </summary>
public class AppConfig
{
    private string _sourceFolder = string.Empty;
    private string _destinationFolder = string.Empty;

    public string SourceFolder
    {
        get => string.IsNullOrEmpty(_sourceFolder) ? GetDefaultFolder() : _sourceFolder;
        set => _sourceFolder = value;
    }

    public string DestinationFolder
    {
        get => string.IsNullOrEmpty(_destinationFolder) ? GetDefaultFolder() : _destinationFolder;
        set => _destinationFolder = value;
    }

    public bool ArchiveEnabled { get; set; } = false;
    public int ArchiveDays { get; set; } = 30;
    public OrganizeMode Mode { get; set; } = OrganizeMode.Manual;
    public List<Rule> Rules { get; set; } = new();
    public List<string> IgnoredFolders { get; set; } = new();

    public static AppConfig CreateDefault()
    {
        return new AppConfig
        {
            ArchiveEnabled = false,
            ArchiveDays = 30,
            Mode = OrganizeMode.Manual,
            Rules = new List<Rule>
            {
                new(".jpg", "图片"), new(".jpeg", "图片"), new(".png", "图片"),
                new(".gif", "图片"), new(".bmp", "图片"), new(".svg", "图片"), new(".webp", "图片"),
                new(".pdf", "文档"), new(".doc", "文档"), new(".docx", "文档"),
                new(".xls", "文档"), new(".xlsx", "文档"), new(".ppt", "文档"),
                new(".pptx", "文档"), new(".txt", "文档"), new(".md", "文档"),
                new(".mp3", "音乐"), new(".wav", "音乐"), new(".flac", "音乐"), new(".aac", "音乐"),
                new(".mp4", "视频"), new(".mov", "视频"), new(".avi", "视频"), new(".mkv", "视频"),
                new(".zip", "压缩包"), new(".rar", "压缩包"), new(".7z", "压缩包"),
                new(".tar", "压缩包"), new(".gz", "压缩包"),
            },
            IgnoredFolders = new List<string> { "图片", "文档", "音乐", "视频", "压缩包", "归档" }
        };
    }

    private static string GetDefaultFolder()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }
}
