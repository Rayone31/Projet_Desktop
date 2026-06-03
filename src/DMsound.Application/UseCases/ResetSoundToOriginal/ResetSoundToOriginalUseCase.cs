using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.ResetSoundToOriginal;

public sealed class ResetSoundToOriginalUseCase
{
    private readonly ISoundboardRepository _repository;

    public ResetSoundToOriginalUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId, string? pendingModifiedFilePath = null)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);

        if (!string.IsNullOrWhiteSpace(pendingModifiedFilePath)
            && !string.Equals(pendingModifiedFilePath.Trim(), sound.InitialFilePath, StringComparison.OrdinalIgnoreCase)
            && File.Exists(pendingModifiedFilePath))
        {
            File.Delete(pendingModifiedFilePath);
        }

        sound.RestoreInitialFilePath();
        _repository.Update(soundboard);
    }
}