using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.PlaySoundByHotkey;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class PlaySoundByHotkeyUseCaseTests
{
    [Fact]
    public void Execute_plays_sound_matching_hotkey()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        soundboard.AddSound(new Sound(SoundId.New(), "Kick", "kick.wav", new Hotkey("A")));
        repository.Add(soundboard);
        var playbackService = new FakeSoundPlaybackService();
        var useCase = new PlaySoundByHotkeyUseCase(repository, playbackService);

        var played = useCase.Execute(soundboard.Id, "a");

        Assert.True(played);
        Assert.Equal("kick.wav", playbackService.LastPlayedFilePath);
    }

    [Fact]
    public void Execute_returns_false_when_hotkey_is_unassigned()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        soundboard.AddSound(new Sound(SoundId.New(), "Kick", "kick.wav", new Hotkey("A")));
        repository.Add(soundboard);
        var playbackService = new FakeSoundPlaybackService();
        var useCase = new PlaySoundByHotkeyUseCase(repository, playbackService);

        var played = useCase.Execute(soundboard.Id, "z");

        Assert.False(played);
        Assert.Null(playbackService.LastPlayedFilePath);
    }

    private sealed class FakeSoundPlaybackService : ISoundPlaybackService
    {
        public string? LastPlayedFilePath { get; private set; }

        public int? SelectedOutputDeviceNumber { get; private set; }

        public void Play(string filePath)
        {
            LastPlayedFilePath = filePath;
        }

        public IReadOnlyList<AudioOutputDevice> GetOutputDevices()
        {
            return [new AudioOutputDevice(0, "Default")];
        }

        public int? GetSelectedOutputDeviceNumber()
        {
            return SelectedOutputDeviceNumber;
        }

        public void SelectOutputDevice(int deviceNumber)
        {
            SelectedOutputDeviceNumber = deviceNumber;
        }
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