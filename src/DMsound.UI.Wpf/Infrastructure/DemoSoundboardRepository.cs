using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.UI.Wpf.Infrastructure;

internal sealed class DemoSoundboardRepository : ISoundboardRepository
{
    private readonly List<Soundboard> _soundboards = new();

    private DemoSoundboardRepository()
    {
    }

    public static DemoSoundboardRepository Create()
    {
        var repository = new DemoSoundboardRepository();
        repository.Load();
        return repository;
    }

    public void Add(Soundboard soundboard)
    {
        _soundboards.Add(soundboard);
    }

    public Soundboard? GetById(SoundboardId id)
    {
        return _soundboards.FirstOrDefault(item => item.Id == id);
    }

    public IReadOnlyCollection<Soundboard> GetAll()
    {
        return _soundboards;
    }

    public bool ExistsByName(string name)
    {
        return _soundboards.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private void Load()
    {
        DemoSoundAssetFactory.EnsureAudioOriginalsFolder();
        DemoSoundAssetFactory.EnsureAudioTrimmedFolder();

        // Créer la soundboard Gaming avec les sons
        var gaming = new Soundboard(SoundboardId.New(), "Gaming");

        // Charger chaque son original et vérifier s'il y a une version trimmed
        var sounds = new[]
        {
            ("sncf.mp3", "sncf", "A"),
            ("fah.mp3", "fah", "S"),
            ("discord-notif.mp3", "discord-notif", "D")
        };

        foreach (var (fileName, soundName, hotkeyChar) in sounds)
        {
            var originalPath = DemoSoundAssetFactory.TryGetOriginalAudioPath(fileName);

            if (originalPath is null)
            {
                continue;
            }

            var trimmedPath = DemoSoundAssetFactory.GetTrimmedAudioPath(fileName);

            var filePath = trimmedPath ?? originalPath;

            gaming.AddSound(new Sound(SoundId.New(), soundName, filePath, new Hotkey(hotkeyChar)));
        }

        _soundboards.Add(gaming);
    }
}