using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.AssignHotkey;

public sealed class AssignHotkeyUseCase
{
    private readonly ISoundboardRepository _repository;

    public AssignHotkeyUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId, string hotkey)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        soundboard.AssignHotkey(soundId, new Hotkey(hotkey));
    }
}