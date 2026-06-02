using System.Security.Cryptography;
using System.Text;

namespace DMsound.Session.Domain;

public sealed record SessionCode
{
    private const int Length = 8;
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public SessionCode(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException("Session code must contain exactly 8 alphanumeric characters.", nameof(value));
        }

        Value = value.ToUpperInvariant();
    }

    public string Value { get; }

    public static SessionCode Generate()
    {
        Span<char> buffer = stackalloc char[Length];

        for (var index = 0; index < buffer.Length; index++)
        {
            buffer[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new SessionCode(new string(buffer));
    }

    public static bool IsValid(string? value)
    {
        return value is not null
            && value.Length == Length
            && value.All(char.IsLetterOrDigit);
    }

    public override string ToString() => Value;
}