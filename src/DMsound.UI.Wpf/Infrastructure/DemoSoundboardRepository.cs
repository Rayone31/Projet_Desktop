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
        repository.Seed();
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

    private void Seed()
    {
        var audioFolder = DemoSoundAssetFactory.EnsureDemoAudioFolder();

        var gaming = new Soundboard(SoundboardId.New(), "Gaming");
        gaming.AddSound(new Sound(SoundId.New(), "sncf", DemoSoundAssetFactory.GetDemoMp3Path(audioFolder, "sncf.mp3"), new Hotkey("A")));
        gaming.AddSound(new Sound(SoundId.New(), "fah", DemoSoundAssetFactory.GetDemoMp3Path(audioFolder, "fah.mp3"), new Hotkey("S")));
        gaming.AddSound(new Sound(SoundId.New(), "discord-notif", DemoSoundAssetFactory.GetDemoMp3Path(audioFolder, "discord-notif.mp3"), new Hotkey("D")));

        _soundboards.Add(gaming);
    }
}