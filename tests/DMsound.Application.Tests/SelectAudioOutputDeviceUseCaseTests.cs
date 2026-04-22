using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Application.UseCases.SelectAudioOutputDevice;

namespace DMsound.Application.Tests;

public sealed class SelectAudioOutputDeviceUseCaseTests
{
    [Fact]
    public void Execute_selects_output_device()
    {
        var service = new FakeSoundPlaybackService();
        var useCase = new SelectAudioOutputDeviceUseCase(service);

        useCase.Execute(2);

        Assert.Equal(2, service.SelectedOutputDeviceNumber);
    }

    private sealed class FakeSoundPlaybackService : ISoundPlaybackService
    {
        public int? SelectedOutputDeviceNumber { get; private set; }

        public void Play(string filePath)
        {
        }

        public IReadOnlyList<AudioOutputDevice> GetOutputDevices()
        {
            return [
                new AudioOutputDevice(0, "Default"),
                new AudioOutputDevice(1, "VoiceMeeter Input"),
                new AudioOutputDevice(2, "Cable Input")
            ];
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
}