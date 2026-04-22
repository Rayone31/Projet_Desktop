using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.CreateSoundboard;

public sealed class CreateSoundboardUseCase
{
    private readonly ISoundboardRepository _repository;

    public CreateSoundboardUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public Soundboard Execute(string name)
    {
        if (_repository.ExistsByName(name))
        {
            throw new InvalidOperationException("Une soundboard portant ce nom existe deja.");
        }

        var soundboard = new Soundboard(SoundboardId.New(), name);
        _repository.Add(soundboard);

        return soundboard;
    }
}