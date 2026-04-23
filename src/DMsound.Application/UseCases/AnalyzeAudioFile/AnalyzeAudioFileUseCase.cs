using DMsound.Application.Abstractions;
using DMsound.Application.Models;

namespace DMsound.Application.UseCases.AnalyzeAudioFile;

public sealed class AnalyzeAudioFileUseCase
{
    private const int DefaultPeakCount = 96;

    private readonly IAudioEditorService _audioEditorService;

    public AnalyzeAudioFileUseCase(IAudioEditorService audioEditorService)
    {
        _audioEditorService = audioEditorService;
    }

    public AudioWaveformAnalysis Execute(string filePath)
    {
        return _audioEditorService.AnalyzeWaveform(filePath, DefaultPeakCount);
    }
}