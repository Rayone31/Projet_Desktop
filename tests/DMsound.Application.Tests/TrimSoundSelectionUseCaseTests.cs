using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.TrimSoundSelection;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class TrimSoundSelectionUseCaseTests
{
    [Fact]
    public void Execute_returns_trimmed_file_path_without_saving_it()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var audioEditorService = new FakeAudioEditorService();
        var useCase = new TrimSoundSelectionUseCase(repository, audioEditorService);

        var result = useCase.Execute(soundboard.Id, sound.Id, 1d, 3d);

        Assert.Equal("kick-trimmed.wav", result);
        Assert.Equal("kick.wav", sound.FilePath);
        Assert.Equal(TimeSpan.FromSeconds(1d), audioEditorService.LastStart);
        Assert.Equal(TimeSpan.FromSeconds(3d), audioEditorService.LastEnd);
    }

    private sealed class FakeAudioEditorService : IAudioEditorService
    {
        public TimeSpan? LastStart { get; private set; }

        public TimeSpan? LastEnd { get; private set; }

        public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
        {
            return new AudioWaveformAnalysis(1d, [10d]);
        }

        public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
        {
        }

        public string TrimSegment(string filePath, TimeSpan start, TimeSpan end)
        {
            LastStart = start;
            LastEnd = end;
            return "kick-trimmed.wav";
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