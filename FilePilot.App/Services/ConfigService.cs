using System;
using System.IO;
using System.Text.Json;
using FilePilot.App.Models;

namespace FilePilot.App.Services;

/// <summary>
/// Handles loading and saving the application configuration to/from JSON.
/// Thread-safe via lock.
/// </summary>
public class ConfigService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".filepilot");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _lock = new();
    private AppConfig? _cachedConfig;

    public AppConfig LoadConfig()
    {
        lock (_lock)
        {
            if (_cachedConfig != null)
                return CloneConfig(_cachedConfig);

            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    _cachedConfig = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions)
                                    ?? AppConfig.CreateDefault();
                }
                else
                {
                    _cachedConfig = AppConfig.CreateDefault();
                    SaveConfigInternal(_cachedConfig);
                }
            }
            catch (Exception)
            {
                _cachedConfig = AppConfig.CreateDefault();
            }

            return CloneConfig(_cachedConfig);
        }
    }

    public void SaveConfig(AppConfig config)
    {
        lock (_lock)
        {
            _cachedConfig = CloneConfig(config);
            SaveConfigInternal(_cachedConfig);
        }
    }

    private void SaveConfigInternal(AppConfig config)
    {
        try
        {
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
        }
    }

    /// <summary>
    /// Returns a deep clone to prevent external modifications to the cached config.
    /// </summary>
    private static AppConfig CloneConfig(AppConfig source)
    {
        var json = JsonSerializer.Serialize(source, JsonOptions);
        return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions)!;
    }

    public string GetConfigFilePath() => ConfigPath;
}
