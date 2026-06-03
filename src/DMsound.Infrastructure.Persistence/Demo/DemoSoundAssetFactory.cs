namespace DMsound.Infrastructure.Persistence.Demo;

internal static class DemoSoundAssetFactory
{
    private const string DemoAudioFolderName = "Assets\\DemoMp3";
    private const string AudioOriginalsFolder = "Assets\\AudioOriginals";
    private const string AudioTrimmedFolder = "Assets\\AudioTrimmed";

    public static string EnsureAudioOriginalsFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, AudioOriginalsFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string EnsureAudioTrimmedFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, AudioTrimmedFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string? TryGetOriginalAudioPath(string fileName)
    {
        var folder = EnsureAudioOriginalsFolder();
        var outputPath = Path.Combine(folder, fileName);

        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        var projectAssetPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            DemoAudioFolderName,
            fileName));

        if (File.Exists(projectAssetPath))
        {
            Directory.CreateDirectory(folder);
            File.Copy(projectAssetPath, outputPath, overwrite: true);
            return outputPath;
        }

        return null;
    }

    public static string? GetTrimmedAudioPath(string fileName)
    {
        var folder = EnsureAudioTrimmedFolder();
        var trimmedPath = Path.Combine(folder, Path.GetFileNameWithoutExtension(fileName) + "_trimmed.wav");

        return File.Exists(trimmedPath) ? trimmedPath : null;
    }
}
