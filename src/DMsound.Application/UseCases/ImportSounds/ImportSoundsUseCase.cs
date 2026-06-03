using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Domain;

namespace DMsound.Application.UseCases.ImportSounds;

public sealed class ImportSoundsUseCase
{
    private static readonly HashSet<string> SupportedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".aac",
        ".aiff",
        ".flac",
        ".m4a",
        ".mp3",
        ".wav",
        ".wma",
    };

    private readonly ISoundboardRepository _repository;
    private readonly IAudioLibraryStorage _audioLibraryStorage;

    public ImportSoundsUseCase(ISoundboardRepository repository, IAudioLibraryStorage audioLibraryStorage)
    {
        _repository = repository;
        _audioLibraryStorage = audioLibraryStorage;
    }

    public IReadOnlyList<SoundSummary> Execute(SoundboardId soundboardId, IEnumerable<string> filePaths)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var importedSounds = new List<SoundSummary>();

        foreach (var filePath in NormalizeFilePaths(filePaths))
        {
            ValidateAudioFile(filePath);

            var storedFilePath = _audioLibraryStorage.StoreOriginal(filePath);
            var sound = new Sound(
                SoundId.New(),
                Path.GetFileNameWithoutExtension(filePath),
                storedFilePath);

            soundboard.AddSound(sound);
            importedSounds.Add(new SoundSummary(sound.Id, sound.Name, sound.Hotkey));
        }

        _repository.Update(soundboard);
        return importedSounds;
    }

    private static IEnumerable<string> NormalizeFilePaths(IEnumerable<string> filePaths)
    {
        var normalizedPaths = filePaths?.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray()
            ?? throw new ArgumentNullException(nameof(filePaths));

        if (normalizedPaths.Length == 0)
        {
            throw new ArgumentException("Aucun fichier audio n'a ete fourni.", nameof(filePaths));
        }

        return normalizedPaths;
    }

    private static void ValidateAudioFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Le fichier audio importe est introuvable.", filePath);
        }

        var extension = Path.GetExtension(filePath);

        if (string.IsNullOrWhiteSpace(extension) || !SupportedAudioExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Le format audio '{extension}' n'est pas supporte.");
        }
    }
}