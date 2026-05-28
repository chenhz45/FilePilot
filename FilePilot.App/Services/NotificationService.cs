using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FilePilot.App.Services;

public static class NotificationService
{
    public static bool IsAvailable =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static async Task ShowAsync(string title, string message)
    {
        if (!IsAvailable) return;

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                await ShowMacNotification(title, message);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                await ShowWindowsNotification(title, message);
        }
        catch
        {
            // notifications are non-critical
        }
    }

    private static async Task ShowMacNotification(string title, string message)
    {
        var escapedMessage = message.Replace("\"", "\\\"");
        var escapedTitle = title.Replace("\"", "\\\"");
        var script = $"-e \"display notification \\\"{escapedMessage}\\\" with title \\\"{escapedTitle}\\\" sound name \\\"default\\\"\"";

        var psi = new ProcessStartInfo
        {
            FileName = "/usr/bin/osascript",
            Arguments = script,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process != null)
            await process.WaitForExitAsync();
    }

    private static async Task ShowWindowsNotification(string title, string message)
    {
        var escapedMessage = message.Replace("'", "''");
        var escapedTitle = title.Replace("'", "''");

        var psCommand =
            $"[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null; " +
            $"$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02); " +
            $"$xml = New-Object Windows.Data.Xml.Dom.XmlDocument; " +
            $"$xml.LoadXml($template.GetXml()); " +
            $"$xml.GetElementsByTagName('text')[0].AppendChild($xml.CreateTextNode('{escapedTitle}')) | Out-Null; " +
            $"$xml.GetElementsByTagName('text')[1].AppendChild($xml.CreateTextNode('{escapedMessage}')) | Out-Null; " +
            $"$toast = [Windows.UI.Notifications.ToastNotification]::new($xml); " +
            $"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('FilePilot').Show($toast)";

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process != null)
            await process.WaitForExitAsync();
    }
}
