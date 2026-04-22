using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.CreateSoundboard;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class CreateSoundboardUseCaseTests
{
    [Fact]
    public void Execute_creates_and_persists_soundboard()
    {
        var repository = new FakeSoundboardRepository();
        var useCase = new CreateSoundboardUseCase(repository);

        var result = useCase.Execute("Gaming");

        Assert.Equal("Gaming", result.Name);
        Assert.Single(repository.Items);
        Assert.Equal(result.Id, repository.Items[0].Id);
    }

    [Fact]
    public void Execute_rejects_duplicate_soundboard_name()
    {
        var repository = new FakeSoundboardRepository();
        repository.Add(new Soundboard(SoundboardId.New(), "Gaming"));
        var useCase = new CreateSoundboardUseCase(repository);

        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute("Gaming"));

        Assert.Equal("Une soundboard portant ce nom existe deja.", exception.Message);
    }

    private sealed class FakeSoundboardRepository : ISoundboardRepository
    {
        private readonly List<Soundboard> _items = new();

        public IReadOnlyList<Soundboard> Items => _items;

        public void Add(Soundboard soundboard)
        {
            _items.Add(soundboard);
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