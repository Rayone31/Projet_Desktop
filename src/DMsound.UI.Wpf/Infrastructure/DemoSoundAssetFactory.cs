using System.IO;

namespace DMsound.UI.Wpf.Infrastructure;

internal static class DemoSoundAssetFactory
{
    private const string DemoAudioFolderName = "Assets\\DemoMp3";

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