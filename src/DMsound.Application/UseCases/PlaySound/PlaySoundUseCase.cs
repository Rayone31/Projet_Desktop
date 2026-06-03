using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.PlaySound;

public sealed class PlaySoundUseCase
{
    private readonly ISoundboardRepository _repository;
    private readonly ISoundPlaybackService _playbackService;

    public PlaySoundUseCase(ISoundboardRepository repository, ISoundPlaybackService playbackService)
    {
        _repository = repository;
        _playbackService = playbackService;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);
        _playbackService.Play(sound.ModifiedFilePath, muteMicDuringPlayback: true);
    }
}
