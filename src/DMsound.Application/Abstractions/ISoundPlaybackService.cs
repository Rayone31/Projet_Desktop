namespace DMsound.Application.Abstractions;

public interface ISoundPlaybackService : IAudioOutputDeviceService
{
    void Play(string filePath);
}