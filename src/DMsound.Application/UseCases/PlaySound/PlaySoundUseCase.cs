using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.PlaySound;

public sealed class PlaySoundUseCase
{
    private readonly ISoundboardRepository _repository;
    private readonly ISoundPlaybackService _soundPlaybackService;

    public PlaySoundUseCase(ISoundboardRepository repository, ISoundPlaybackService soundPlaybackService)
    {
        _repository = repository;
        _soundPlaybackService = soundPlaybackService;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.Sounds.FirstOrDefault(item => item.Id == soundId)
            ?? throw new InvalidOperationException("Le son demande est introuvable.");

        _soundPlaybackService.Play(sound.FilePath);
    }
}