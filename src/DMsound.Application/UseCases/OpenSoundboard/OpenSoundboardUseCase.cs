using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.OpenSoundboard;

public sealed class OpenSoundboardUseCase
{
    private readonly ISoundboardRepository _repository;

    public OpenSoundboardUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public Soundboard Execute(SoundboardId soundboardId)
    {
        var soundboard = _repository.GetById(soundboardId);

        if (soundboard is null)
        {
            throw new InvalidOperationException("La soundboard demandee est introuvable.");
        }

        return soundboard;
    }
}