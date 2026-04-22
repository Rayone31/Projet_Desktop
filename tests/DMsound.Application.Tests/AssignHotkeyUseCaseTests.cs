using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.AssignHotkey;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class AssignHotkeyUseCaseTests
{
    [Fact]
    public void Execute_assigns_hotkey_to_sound()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var useCase = new AssignHotkeyUseCase(repository);

        useCase.Execute(soundboard.Id, sound.Id, "k");

        Assert.Equal("K", soundboard.GetSoundById(sound.Id).Hotkey?.Value);
    }

    [Fact]
    public void Execute_rejects_duplicate_hotkey()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var first = new Sound(SoundId.New(), "Kick", "kick.wav", new Hotkey("A"));
        var second = new Sound(SoundId.New(), "Snare", "snare.wav");
        soundboard.AddSound(first);
        soundboard.AddSound(second);
        repository.Add(soundboard);
        var useCase = new AssignHotkeyUseCase(repository);

        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute(soundboard.Id, second.Id, "a"));

        Assert.Equal("La touche est deja utilisee dans cette soundboard.", exception.Message);
    }

    private sealed class FakeSoundboardRepository : ISoundboardRepository
    {
        private readonly List<Soundboard> _items = new();

        public void Add(Soundboard soundboard)
        {
            _items.Add(soundboard);
        }

        public Soundboard? GetById(SoundboardId id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }

        public IReadOnlyCollection<Soundboard> GetAll()
        {
            return _items;
        }

        public bool ExistsByName(string name)
        {
            return _items.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}