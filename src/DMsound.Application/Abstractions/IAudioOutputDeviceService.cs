using DMsound.Application.Models;

namespace DMsound.Application.Abstractions;

public interface IAudioOutputDeviceService
{
    IReadOnlyList<AudioOutputDevice> GetOutputDevices();

    int? GetSelectedOutputDeviceNumber();

    void SelectOutputDevice(int deviceNumber);
}