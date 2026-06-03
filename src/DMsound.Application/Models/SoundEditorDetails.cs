using DMsound.Domain;

namespace DMsound.Application.Models;

public sealed record SoundEditorDetails(
    SoundId Id,
    string Name,
    string InitialFilePath,
    string ModifiedFilePath,
    double DurationSeconds,
    IReadOnlyList<double> WaveformPeaks);
