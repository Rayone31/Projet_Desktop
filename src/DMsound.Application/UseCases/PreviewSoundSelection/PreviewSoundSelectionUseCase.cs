using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.PreviewSoundSelection;

public sealed class PreviewSoundSelectionUseCase
{
    private readonly ISoundboardRepository _repository;
    private readonly IAudioEditorService _audioEditorService;

    public PreviewSoundSelectionUseCase(ISoundboardRepository repository, IAudioEditorService audioEditorService)
    {
        _repository = repository;
        _audioEditorService = audioEditorService;
    }

    public void Execute(SoundboardId soundboardId, SoundId soundId, double startSeconds, double endSeconds)
    {
        var sound = GetSound(soundboardId, soundId);
        ValidateRange(startSeconds, endSeconds);

        _audioEditorService.PreviewSegment(sound.ModifiedFilePath, TimeSpan.FromSeconds(startSeconds), TimeSpan.FromSeconds(endSeconds));
    }

    private Sound GetSound(SoundboardId soundboardId, SoundId soundId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        return soundboard.GetSoundById(soundId);
    }

    private static void ValidateRange(double startSeconds, double endSeconds)
    {
        if (startSeconds < 0 || endSeconds <= startSeconds)
        {
            throw new ArgumentException("La plage audio selectionnee est invalide.");
        }
    }
}