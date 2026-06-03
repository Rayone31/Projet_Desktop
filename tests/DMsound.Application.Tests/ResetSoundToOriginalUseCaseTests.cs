using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.ResetSoundToOriginal;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class ResetSoundToOriginalUseCaseTests
{
    [Fact]
    public void Execute_restores_sound_to_original_file_path()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.mp3");
        var editedFilePath = CreateEditedFile();
        sound.UpdateModifiedFilePath(editedFilePath);
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var useCase = new ResetSoundToOriginalUseCase(repository);

        useCase.Execute(soundboard.Id, sound.Id, editedFilePath);

        Assert.Equal("kick.mp3", sound.ModifiedFilePath);
        Assert.False(File.Exists(editedFilePath));
    }

    private static string CreateEditedFile()
    {
        var folder = Path.Combine(Path.GetTempPath(), "dmsound-reset-tests");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, "kick_trimmed.wav");
        File.WriteAllText(filePath, string.Empty);
        return filePath;
    }

    private sealed class FakeSoundboardRepository : ISoundboardRepository
    {
        private readonly List<Soundboard> _items = new();

        public void Add(Soundboard soundboard)
        {
            _items.Add(soundboard);
        }

        public void Update(Soundboard soundboard)
        {
            // No-op for test
        }

        public Soundboard? GetById(SoundboardId id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }

        public IReadOnlyCollection<Soundboard> GetAll()
        {
            return _items;
        }

        public bool ExistsByName(string name)
        {
            return _items.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}