using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.GetSoundEditorDetails;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class GetSoundEditorDetailsUseCaseTests
{
    [Fact]
    public void Execute_returns_editor_details_with_waveform()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var audioEditorService = new FakeAudioEditorService();
        var useCase = new GetSoundEditorDetailsUseCase(repository, audioEditorService);

        var result = useCase.Execute(soundboard.Id, sound.Id);

        Assert.Equal("Kick", result.Name);
        Assert.Equal("kick.wav", result.InitialFilePath);
        Assert.Equal("kick.wav", result.ModifiedFilePath);
        Assert.Equal(12.5d, result.DurationSeconds);
        Assert.Equal([10d, 25d, 50d], result.WaveformPeaks);
    }

    private sealed class FakeAudioEditorService : IAudioEditorService
    {
        public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
        {
            return new AudioWaveformAnalysis(12.5d, [10d, 25d, 50d]);
        }

        public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
        {
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