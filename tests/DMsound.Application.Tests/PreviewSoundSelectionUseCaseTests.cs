using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.PreviewSoundSelection;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class PreviewSoundSelectionUseCaseTests
{
    [Fact]
    public void Execute_calls_audio_editor_service_with_selection_range()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var audioEditorService = new FakeAudioEditorService();
        var useCase = new PreviewSoundSelectionUseCase(repository, audioEditorService);

        useCase.Execute(soundboard.Id, sound.Id, 0.5d, 2d);

        Assert.Equal("kick.wav", audioEditorService.LastFilePath);
        Assert.Equal(TimeSpan.FromSeconds(0.5d), audioEditorService.LastStart);
        Assert.Equal(TimeSpan.FromSeconds(2d), audioEditorService.LastEnd);
    }

    [Fact]
    public void Execute_rejects_invalid_selection_range()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var audioEditorService = new FakeAudioEditorService();
        var useCase = new PreviewSoundSelectionUseCase(repository, audioEditorService);

        var exception = Assert.Throws<ArgumentException>(() => useCase.Execute(soundboard.Id, sound.Id, 2d, 1d));

        Assert.Equal("La plage audio selectionnee est invalide.", exception.Message);
    }

    private sealed class FakeAudioEditorService : IAudioEditorService
    {
        public string? LastFilePath { get; private set; }

        public TimeSpan? LastStart { get; private set; }

        public TimeSpan? LastEnd { get; private set; }

        public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
        {
            return new AudioWaveformAnalysis(1d, [10d]);
        }

        public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
        {
            LastFilePath = filePath;
            LastStart = start;
            LastEnd = end;
        }

        public string TrimSegment(string filePath, TimeSpan start, TimeSpan end)
        {
            return filePath;
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