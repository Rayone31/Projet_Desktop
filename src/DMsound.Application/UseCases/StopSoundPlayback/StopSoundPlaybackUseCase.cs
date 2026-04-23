using DMsound.Application.Abstractions;

namespace DMsound.Application.UseCases.StopSoundPlayback;

public sealed class StopSoundPlaybackUseCase
{
    private readonly ISoundPlaybackService _playbackService;

    public StopSoundPlaybackUseCase(ISoundPlaybackService playbackService)
    {
        _playbackService = playbackService;
    }

    public void Execute()
    {
        _playbackService.Stop();
    }
}