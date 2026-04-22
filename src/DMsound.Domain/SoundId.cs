namespace DMsound.Domain;

public readonly record struct SoundId(Guid Value)
{
    public static SoundId New() => new(Guid.NewGuid());
}