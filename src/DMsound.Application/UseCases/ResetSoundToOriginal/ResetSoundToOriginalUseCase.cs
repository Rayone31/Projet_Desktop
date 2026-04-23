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

    public void Execute(SoundboardId soundboardId, SoundId soundId, string? editedFilePath = null)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);

        if (!string.IsNullOrWhiteSpace(editedFilePath)
            && !string.Equals(editedFilePath.Trim(), sound.OriginalFilePath, StringComparison.OrdinalIgnoreCase)
            && File.Exists(editedFilePath))
        {
            File.Delete(editedFilePath);
        }

        sound.RestoreOriginalFilePath();
    }
}