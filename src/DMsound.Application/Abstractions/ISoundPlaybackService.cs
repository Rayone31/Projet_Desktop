namespace DMsound.Application.Abstractions;

public interface ISoundPlaybackService : IAudioOutputDeviceService, IAudioEditorService
{
    void Play(string filePath);

    void Play(string filePath, bool muteMicDuringPlayback);

    void Stop();

    bool IsMicMuteSupported();
}
