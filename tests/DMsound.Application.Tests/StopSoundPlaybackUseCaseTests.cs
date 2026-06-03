using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.StopSoundPlayback;

namespace DMsound.Application.Tests;

public sealed class StopSoundPlaybackUseCaseTests
{
    [Fact]
    public void Execute_calls_stop_on_playback_service()
    {
        var service = new FakeSoundPlaybackService();
        var useCase = new StopSoundPlaybackUseCase(service);

        useCase.Execute();

        Assert.True(service.StopCalled);
    }

    private sealed class FakeSoundPlaybackService : ISoundPlaybackService
    {
        public bool StopCalled { get; private set; }

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
            StopCalled = true;
        }

        public void Play(string filePath, bool muteMicDuringPlayback) { }

        public bool IsMicMuteSupported() => false;

        public void Play(string filePath)
        {
        }

        public IReadOnlyList<AudioOutputDevice> GetOutputDevices()
        {
            return [new AudioOutputDevice(0, "Default")];
        }

        public int? GetSelectedOutputDeviceNumber()
        {
            return 0;
        }

        public void SelectOutputDevice(int deviceNumber)
        {
        }
    }
}