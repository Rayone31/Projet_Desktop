using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.ImportSounds;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class ImportSoundsUseCaseTests
{
    [Fact]
    public void Execute_imports_supported_audio_files_into_soundboard()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        repository.Add(soundboard);
        var useCase = new ImportSoundsUseCase(repository, new PassthroughAudioLibraryStorage());
        var filePath = CreateAudioFile("victory.mp3");

        var importedSounds = useCase.Execute(soundboard.Id, [filePath]);

        Assert.Single(importedSounds);
        Assert.Equal("victory", importedSounds[0].Name);
        Assert.Single(soundboard.Sounds);
        var importedSound = soundboard.Sounds.First();
        Assert.Equal("victory", importedSound.Name);
        Assert.True(File.Exists(importedSound.InitialFilePath));
        Assert.Equal(importedSound.InitialFilePath, importedSound.ModifiedFilePath);
    }

    [Fact]
    public void Execute_rejects_unsupported_audio_formats()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        repository.Add(soundboard);
        var useCase = new ImportSoundsUseCase(repository, new PassthroughAudioLibraryStorage());
        var filePath = CreateAudioFile("notes.txt");

        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute(soundboard.Id, [filePath]));

        Assert.Equal("Le format audio '.txt' n'est pas supporte.", exception.Message);
        Assert.Empty(soundboard.Sounds);
    }

    private static string CreateAudioFile(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "dmsound-import-tests");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, fileName);
        File.WriteAllText(filePath, string.Empty);
        return filePath;
    }

    private sealed class PassthroughAudioLibraryStorage : IAudioLibraryStorage
    {
        public string StoreOriginal(string sourceFilePath) => sourceFilePath;
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