using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Domain;

namespace DMsound.Application.UseCases.GetSoundboardDetails;

public sealed class GetSoundboardDetailsUseCase
{
    private readonly ISoundboardRepository _repository;

    public GetSoundboardDetailsUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public SoundboardDetails Execute(SoundboardId soundboardId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sounds = soundboard.Sounds
            .Select(sound => new SoundSummary(sound.Id, sound.Name, sound.Hotkey, sound.IsEnabled))
            .ToArray();

        return new SoundboardDetails(soundboard.Id, soundboard.Name, sounds);
    }
}
