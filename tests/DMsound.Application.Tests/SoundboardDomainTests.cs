using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class SoundboardDomainTests
{
    [Fact]
    public void AddSound_rejects_duplicate_hotkey()
    {
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var first = new Sound(SoundId.New(), "Kick", "kick.mp3", new Hotkey("A"));
        var second = new Sound(SoundId.New(), "Snare", "snare.mp3", new Hotkey("a"));

        soundboard.AddSound(first);

        var exception = Assert.Throws<InvalidOperationException>(() => soundboard.AddSound(second));

        Assert.Equal("La touche est deja utilisee dans cette soundboard.", exception.Message);
    }

    [Fact]
    public void Sound_rename_updates_name()
    {
        var sound = new Sound(SoundId.New(), "Kick", "kick.mp3");

        sound.Rename("Bass Kick");

        Assert.Equal("Bass Kick", sound.Name);
    }
}