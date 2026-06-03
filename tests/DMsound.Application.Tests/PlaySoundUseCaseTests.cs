using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.PlaySound;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class PlaySoundUseCaseTests
{
    [Fact]
    public void Execute_plays_the_selected_sound_file()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var playbackService = new FakeSoundPlaybackService();
        var useCase = new PlaySoundUseCase(repository, playbackService);

        useCase.Execute(soundboard.Id, sound.Id);

        Assert.Equal("kick.wav", playbackService.LastPlayedFilePath);
    }

    [Fact]
    public void Execute_throws_when_sound_is_missing()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        repository.Add(soundboard);
        var playbackService = new FakeSoundPlaybackService();
        var useCase = new PlaySoundUseCase(repository, playbackService);

        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute(soundboard.Id, SoundId.New()));

        Assert.Equal("Le son demande est introuvable.", exception.Message);
    }

    private sealed class FakeSoundPlaybackService : ISoundPlaybackService
    {
        public string? LastPlayedFilePath { get; private set; }

        public int? SelectedOutputDeviceNumber { get; private set; }
        
            public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
            {
                return new AudioWaveformAnalysis(0d, []);
            }
        
            public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
            {
            }
        
            public string TrimSegment(string filePath, TimeSpan start, TimeSpan end)
            {
                return filePath;
            }

            public void Stop()
            {
            }

        public void Play(string filePath, bool muteMicDuringPlayback) { }

        public bool IsMicMuteSupported() => false;

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

        public void Update(Soundboard soundboard)
        {
            // No-op for test
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