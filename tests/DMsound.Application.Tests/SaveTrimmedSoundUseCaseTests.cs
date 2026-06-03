using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.SaveTrimmedSound;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class SaveTrimmedSoundUseCaseTests
{
    [Fact]
    public void Execute_updates_sound_path_with_trimmed_file_path()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        var sound = new Sound(SoundId.New(), "Kick", "kick.wav");
        soundboard.AddSound(sound);
        repository.Add(soundboard);
        var useCase = new SaveTrimmedSoundUseCase(repository);

        useCase.Execute(soundboard.Id, sound.Id, "kick-trimmed.wav");

        Assert.Equal("kick-trimmed.wav", sound.ModifiedFilePath);
        Assert.Equal("kick.wav", sound.InitialFilePath);
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