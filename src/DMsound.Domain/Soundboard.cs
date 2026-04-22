namespace DMsound.Domain;

public sealed class Soundboard
{
    private readonly List<Sound> _sounds = new();

    public Soundboard(SoundboardId id, string name, bool isVisible = true)
    {
        Id = id;
        Name = ValidateName(name);
        IsVisible = isVisible;
    }

    public SoundboardId Id { get; }

    public string Name { get; private set; }

    public bool IsVisible { get; private set; }

    public IReadOnlyCollection<Sound> Sounds => _sounds.AsReadOnly();

    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
    }

    public void AddSound(Sound sound)
    {
        EnsureHotkeyIsUnique(sound);
        _sounds.Add(sound);
    }

    public Sound GetSoundById(SoundId soundId)
    {
        return _sounds.FirstOrDefault(sound => sound.Id == soundId)
            ?? throw new InvalidOperationException("Le son demande est introuvable.");
    }

    public Sound? FindSoundByHotkey(Hotkey hotkey)
    {
        return _sounds.FirstOrDefault(sound => sound.Hotkey == hotkey);
    }

    public void AssignHotkey(SoundId soundId, Hotkey hotkey)
    {
        var sound = GetSoundById(soundId);
        EnsureHotkeyIsUniqueForAnotherSound(sound, hotkey);
        sound.AssignHotkey(hotkey);
    }

    private void EnsureHotkeyIsUnique(Sound sound)
    {
        if (sound.Hotkey is null)
        {
            return;
        }

        if (_sounds.Any(existing => existing.Hotkey == sound.Hotkey))
        {
            throw new InvalidOperationException("La touche est deja utilisee dans cette soundboard.");
        }
    }

    private void EnsureHotkeyIsUniqueForAnotherSound(Sound targetSound, Hotkey hotkey)
    {
        var duplicateExists = _sounds.Any(sound => sound.Id != targetSound.Id && sound.Hotkey == hotkey);

        if (duplicateExists)
        {
            throw new InvalidOperationException("La touche est deja utilisee dans cette soundboard.");
        }
    }

    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Le nom de la soundboard ne peut pas etre vide.", nameof(value));
        }

        return value.Trim();
    }
}