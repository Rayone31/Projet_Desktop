using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.ToggleSound;

public sealed class ToggleSoundUseCase
{
    private readonly ISoundboardRepository _repository;

    public ToggleSoundUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public bool Execute(SoundboardId soundboardId, SoundId soundId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);

        if (sound.IsEnabled)
        {
            sound.Disable();
        }
        else
        {
            sound.Enable();
        }

        _repository.Update(soundboard);
        return sound.IsEnabled;
    }
}
