using DMsound.Domain;

namespace DMsound.Application.Models;

public sealed record SoundEditorDetails(
    SoundId Id,
    string Name,
    string OriginalFilePath,
    string FilePath,
    double DurationSeconds,
    IReadOnlyList<double> WaveformPeaks);