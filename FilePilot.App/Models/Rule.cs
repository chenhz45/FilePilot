using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace FilePilot.App.Models;

/// <summary>
/// Represents a file extension-to-folder mapping rule.
/// Observable for UI binding support.
/// </summary>
public partial class Rule : ObservableObject
{
    [ObservableProperty]
    private string _extension = string.Empty;

    [ObservableProperty]
    private string _targetFolder = string.Empty;

    [ObservableProperty]
    private bool _enabled = false;

    [JsonConstructor]
    public Rule() { }

    public Rule(string extension, string targetFolder, bool enabled = false)
    {
        _extension = extension.StartsWith('.') ? extension : $".{extension}";
        _targetFolder = targetFolder;
        _enabled = enabled;
    }

    partial void OnEnabledChanged(bool value) => OnPropertyChanged(nameof(Enabled));
}
