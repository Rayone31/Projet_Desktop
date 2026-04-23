using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.TrimSoundSelection;

public sealed class TrimSoundSelectionUseCase
{
    private readonly ISoundboardRepository _repository;
    private readonly IAudioEditorService _audioEditorService;

    public TrimSoundSelectionUseCase(ISoundboardRepository repository, IAudioEditorService audioEditorService)
    {
        _repository = repository;
        _audioEditorService = audioEditorService;
    }

    public string Execute(SoundboardId soundboardId, SoundId soundId, double startSeconds, double endSeconds)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);
        ValidateRange(startSeconds, endSeconds);

        return _audioEditorService.TrimSegment(
            sound.FilePath,
            TimeSpan.FromSeconds(startSeconds),
            TimeSpan.FromSeconds(endSeconds));
    }

    private static void ValidateRange(double startSeconds, double endSeconds)
    {
        if (startSeconds < 0 || endSeconds <= startSeconds)
        {
            throw new ArgumentException("La plage audio selectionnee est invalide.");
        }
    }
}