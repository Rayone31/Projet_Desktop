using DMsound.Domain;

namespace DMsound.Application.Models;

public sealed record SoundSummary(SoundId Id, string Name, string FilePath, Hotkey? Hotkey);