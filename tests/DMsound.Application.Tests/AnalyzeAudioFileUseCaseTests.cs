using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.AnalyzeAudioFile;

namespace DMsound.Application.Tests;

public sealed class AnalyzeAudioFileUseCaseTests
{
    [Fact]
    public void Execute_returns_waveform_analysis_for_file()
    {
        var service = new FakeAudioEditorService();
        var useCase = new AnalyzeAudioFileUseCase(service);

        var result = useCase.Execute("kick.wav");

        Assert.Equal(9d, result.DurationSeconds);
        Assert.Equal([10d, 20d], result.WaveformPeaks);
    }

    private sealed class FakeAudioEditorService : IAudioEditorService
    {
        public AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount)
        {
            return new AudioWaveformAnalysis(9d, [10d, 20d]);
        }

        public void PreviewSegment(string filePath, TimeSpan start, TimeSpan end)
        {
        }

        public string TrimSegment(string filePath, TimeSpan start, TimeSpan end)
        {
            return filePath;
        }
    }
}