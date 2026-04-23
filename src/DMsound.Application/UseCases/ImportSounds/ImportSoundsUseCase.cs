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

    public ImportSoundsUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<SoundSummary> Execute(SoundboardId soundboardId, IEnumerable<string> filePaths)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var importedSounds = new List<SoundSummary>();

        foreach (var filePath in NormalizeFilePaths(filePaths))
        {
            ValidateAudioFile(filePath);

            var sound = new Sound(
                SoundId.New(),
                Path.GetFileNameWithoutExtension(filePath),
                filePath);

            soundboard.AddSound(sound);
            importedSounds.Add(new SoundSummary(sound.Id, sound.Name, sound.FilePath, sound.Hotkey));
        }

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