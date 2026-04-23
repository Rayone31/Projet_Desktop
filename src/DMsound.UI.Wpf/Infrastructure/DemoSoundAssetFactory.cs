using System.IO;

namespace DMsound.UI.Wpf.Infrastructure;

internal static class DemoSoundAssetFactory
{
    private const string DemoAudioFolderName = "Assets\\DemoMp3";
    private const string AudioOriginalsFolder = "Assets\\AudioOriginals";
    private const string AudioTrimmedFolder = "Assets\\AudioTrimmed";

    /// <summary>Garantit l'existence du dossier des sons originaux</summary>
    public static string EnsureAudioOriginalsFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, AudioOriginalsFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>Garantit l'existence du dossier des sons modifiés</summary>
    public static string EnsureAudioTrimmedFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, AudioTrimmedFolder);
        Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>Retourne le chemin du fichier audio original, ou null si aucun fichier source n'existe.</summary>
    public static string? TryGetOriginalAudioPath(string fileName)
    {
        var folder = EnsureAudioOriginalsFolder();
        var outputPath = Path.Combine(folder, fileName);

        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        // Chercher dans le dossier de projet (DemoMp3)
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

    /// <summary>Retourne le chemin du fichier audio modifié (trimmed), ou null si inexistant</summary>
    public static string? GetTrimmedAudioPath(string fileName)
    {
        var folder = EnsureAudioTrimmedFolder();
        var trimmedPath = Path.Combine(folder, Path.GetFileNameWithoutExtension(fileName) + "_trimmed.wav");

        return File.Exists(trimmedPath) ? trimmedPath : null;
    }

    public static string EnsureDemoAudioFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, DemoAudioFolderName);

        Directory.CreateDirectory(folder);
        return folder;
    }

    public static string GetDemoMp3Path(string folderPath, string fileName)
    {
        var outputPath = Path.Combine(folderPath, fileName);

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
            Directory.CreateDirectory(folderPath);
            File.Copy(projectAssetPath, outputPath, overwrite: true);
            return outputPath;
        }

        throw new FileNotFoundException(
            $"Fichier audio de demo introuvable: {fileName}. Place le fichier dans '{projectAssetPath}' ou '{outputPath}'.",
            outputPath);
    }
}