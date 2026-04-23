namespace DMsound.Application.Models;

public sealed record AudioWaveformAnalysis(double DurationSeconds, IReadOnlyList<double> WaveformPeaks);