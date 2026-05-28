using System.IO;

namespace FilePilot.App.Services;

/// <summary>
/// Handles file name conflicts by generating unique file names.
/// Example: file.jpg → file(1).jpg → file(2).jpg
/// </summary>
public static class ConflictResolver
{
    public static string GetUniqueFilePath(string destDir, string fileName)
    {
        var destPath = Path.Combine(destDir, fileName);

        if (!File.Exists(destPath))
            return destPath;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        do
        {
            var newName = $"{nameWithoutExt}({counter}){extension}";
            destPath = Path.Combine(destDir, newName);
            counter++;
        }
        while (File.Exists(destPath));

        return destPath;
    }
}
