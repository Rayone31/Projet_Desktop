namespace DMsound.Domain;

public readonly record struct SoundboardId(Guid Value)
{
    public static SoundboardId New() => new(Guid.NewGuid());
}