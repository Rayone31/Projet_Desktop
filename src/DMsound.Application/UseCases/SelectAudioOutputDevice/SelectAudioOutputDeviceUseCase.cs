using DMsound.Application.Abstractions;

namespace DMsound.Application.UseCases.SelectAudioOutputDevice;

public sealed class SelectAudioOutputDeviceUseCase
{
    private readonly IAudioOutputDeviceService _audioOutputDeviceService;

    public SelectAudioOutputDeviceUseCase(IAudioOutputDeviceService audioOutputDeviceService)
    {
        _audioOutputDeviceService = audioOutputDeviceService;
    }

    public void Execute(int deviceNumber)
    {
        _audioOutputDeviceService.SelectOutputDevice(deviceNumber);
    }
}