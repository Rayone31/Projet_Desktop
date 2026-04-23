using DMsound.Session.Domain;

namespace DMsound.Session.Tests;

public sealed class SessionCodeTests
{
    [Fact]
    public void Generate_creates_an_8_character_alphanumeric_code()
    {
        var code = SessionCode.Generate();

        Assert.Equal(8, code.Value.Length);
        Assert.All(code.Value, character => Assert.True(char.IsLetterOrDigit(character)));
    }

    [Theory]
    [InlineData("ABCDEFGH")]
    [InlineData("A1B2C3D4")]
    public void Constructor_accepts_valid_codes(string value)
    {
        var code = new SessionCode(value);

        Assert.Equal(value, code.Value);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("INVALID!")]
    public void Constructor_rejects_invalid_codes(string value)
    {
        Assert.Throws<ArgumentException>(() => new SessionCode(value));
    }
}