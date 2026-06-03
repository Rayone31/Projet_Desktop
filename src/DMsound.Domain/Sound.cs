namespace DMsound.Domain;

public sealed class Sound
{
    public Sound(SoundId id, string name, string initialFilePath, Hotkey? hotkey = null, bool isEnabled = true)
    {
        Id = id;
        Name = ValidateText(name, nameof(name));
        InitialFilePath = ValidateText(initialFilePath, nameof(initialFilePath));
        ModifiedFilePath = InitialFilePath;
        Hotkey = hotkey;
        IsEnabled = isEnabled;
    }

    public SoundId Id { get; }

    public string Name { get; private set; }

    public string InitialFilePath { get; }

    public string ModifiedFilePath { get; private set; }

    public Hotkey? Hotkey { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool HasModification =>
        !string.Equals(InitialFilePath, ModifiedFilePath, StringComparison.OrdinalIgnoreCase);

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

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void UpdateModifiedFilePath(string filePath)
    {
        ModifiedFilePath = ValidateText(filePath, nameof(filePath));
    }

    public void RestoreInitialFilePath()
    {
        ModifiedFilePath = InitialFilePath;
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
