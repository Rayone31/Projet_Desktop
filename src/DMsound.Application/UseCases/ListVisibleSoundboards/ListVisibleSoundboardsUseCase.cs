using DMsound.Application.Abstractions;
using DMsound.Application.Models;

namespace DMsound.Application.UseCases.ListVisibleSoundboards;

public sealed class ListVisibleSoundboardsUseCase
{
    private readonly ISoundboardRepository _repository;

    public ListVisibleSoundboardsUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<SoundboardSummary> Execute()
    {
        return _repository.GetAll()
            .Where(soundboard => soundboard.IsVisible)
            .Select(soundboard => new SoundboardSummary(soundboard.Id, soundboard.Name))
            .ToArray();
    }
}