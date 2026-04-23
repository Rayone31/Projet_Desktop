using DMsound.Application.Abstractions;
using DMsound.Application.Models;
using DMsound.Domain;

namespace DMsound.Application.UseCases.GetSoundEditorDetails;

public sealed class GetSoundEditorDetailsUseCase
{
    private const int DefaultPeakCount = 96;

    private readonly ISoundboardRepository _repository;
    private readonly IAudioEditorService _audioEditorService;

    public GetSoundEditorDetailsUseCase(ISoundboardRepository repository, IAudioEditorService audioEditorService)
    {
        _repository = repository;
        _audioEditorService = audioEditorService;
    }

    public SoundEditorDetails Execute(SoundboardId soundboardId, SoundId soundId)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var sound = soundboard.GetSoundById(soundId);
        var waveform = _audioEditorService.AnalyzeWaveform(sound.FilePath, DefaultPeakCount);

        return new SoundEditorDetails(
            sound.Id,
            sound.Name,
            sound.OriginalFilePath,
            sound.FilePath,
            waveform.DurationSeconds,
            waveform.WaveformPeaks);
    }
}