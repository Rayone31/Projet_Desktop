namespace DMsound.Domain;

public readonly record struct Hotkey
{
    public Hotkey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("La touche ne peut pas etre vide.", nameof(value));
        }

        Value = value.Trim().ToUpperInvariant();
    }

    public string Value { get; }
}