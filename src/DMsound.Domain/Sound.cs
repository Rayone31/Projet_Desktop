namespace DMsound.Domain;

public sealed class Sound
{
    public Sound(SoundId id, string name, string filePath, Hotkey? hotkey = null)
    {
        Id = id;
        Name = ValidateText(name, nameof(name));
        FilePath = ValidateText(filePath, nameof(filePath));
        Hotkey = hotkey;
    }

    public SoundId Id { get; }

    public string Name { get; private set; }

    public string FilePath { get; }

    public Hotkey? Hotkey { get; private set; }

    public void Rename(string name)
    {
        Name = ValidateText(name, nameof(name));
    }

    public void AssignHotkey(Hotkey hotkey)
    {
        Hotkey = hotkey;
    }

    public void ClearHotkey()
    {
        Hotkey = null;
    }

    private static string ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("La valeur ne peut pas etre vide.", parameterName);
        }

        return value.Trim();
    }
}