namespace DMsound.Application.Abstractions;

public interface ISoundPlaybackService : IAudioOutputDeviceService, IAudioEditorService
{
    void Play(string filePath);

    void Stop();
}