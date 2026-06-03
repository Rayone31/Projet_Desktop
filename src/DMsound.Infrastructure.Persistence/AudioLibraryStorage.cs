using DMsound.Application.Abstractions;

namespace DMsound.Infrastructure.Persistence;

public sealed class AudioLibraryStorage : IAudioLibraryStorage
{
    private readonly string _libraryDirectory;

    public AudioLibraryStorage(string? libraryDirectory = null)
    {
        _libraryDirectory = libraryDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DMsound",
            "library");
    }

    public string StoreOriginal(string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new ArgumentException("Le chemin du fichier source est invalide.", nameof(sourceFilePath));
        }

        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException(
                "Le fichier audio importe est introuvable. Si le fichier est sur OneDrive, ouvre-le une fois pour le telecharger localement.",
                sourceFilePath);
        }

        Directory.CreateDirectory(_libraryDirectory);

        var safeFileName = Path.GetFileName(sourceFilePath);
        var destinationPath = Path.Combine(_libraryDirectory, $"{Guid.NewGuid():N}_{safeFileName}");

        File.Copy(sourceFilePath, destinationPath, overwrite: false);
        return destinationPath;
    }
}
