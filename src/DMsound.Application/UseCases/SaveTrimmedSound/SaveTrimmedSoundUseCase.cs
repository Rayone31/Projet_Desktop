using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.SaveTrimmedSound;

public sealed class SaveTrimmedSoundUseCase
{
    private readonly ISoundboardRepository _repository;

    public SaveTrimmedSoundUseCase(ISoundboardRepository repository)
    {
        _repository = repository;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId, string trimmedFilePath)
    {
        if (string.IsNullOrWhiteSpace(trimmedFilePath))
        {
            throw new ArgumentException("Le chemin du fichier decoupe est invalide.", nameof(trimmedFilePath));
        }

        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);
        sound.UpdateModifiedFilePath(trimmedFilePath);
        _repository.Update(soundboard);
    }
}