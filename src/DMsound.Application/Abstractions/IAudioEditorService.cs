using DMsound.Application.Models;

namespace DMsound.Application.Abstractions;

public interface IAudioEditorService
{
    AudioWaveformAnalysis AnalyzeWaveform(string filePath, int peakCount);

    void PreviewSegment(string filePath, TimeSpan start, TimeSpan end);

    string TrimSegment(string filePath, TimeSpan start, TimeSpan end);
}