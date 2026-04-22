using DMsound.Application.Abstractions;
using DMsound.Application.Models;

namespace DMsound.Application.UseCases.ListAudioOutputDevices;

public sealed class ListAudioOutputDevicesUseCase
{
    private readonly IAudioOutputDeviceService _audioOutputDeviceService;

    public ListAudioOutputDevicesUseCase(IAudioOutputDeviceService audioOutputDeviceService)
    {
        _audioOutputDeviceService = audioOutputDeviceService;
    }

    public IReadOnlyList<AudioOutputDevice> Execute()
    {
        return _audioOutputDeviceService.GetOutputDevices();
    }
}