using DMsound.Domain;

namespace DMsound.Application.Models;

public sealed record SoundboardDetails(SoundboardId Id, string Name, IReadOnlyList<SoundSummary> Sounds);