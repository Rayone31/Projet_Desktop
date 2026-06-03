using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.OpenSoundboard;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class OpenSoundboardUseCaseTests
{
    [Fact]
    public void Execute_returns_the_requested_soundboard()
    {
        var repository = new FakeSoundboardRepository();
        var soundboard = new Soundboard(SoundboardId.New(), "Gaming");
        repository.Add(soundboard);
        var useCase = new OpenSoundboardUseCase(repository);

        var result = useCase.Execute(soundboard.Id);

        Assert.Equal(soundboard.Id, result.Id);
        Assert.Equal("Gaming", result.Name);
    }

    [Fact]
    public void Execute_throws_when_soundboard_is_missing()
    {
        var repository = new FakeSoundboardRepository();
        var useCase = new OpenSoundboardUseCase(repository);

        var exception = Assert.Throws<InvalidOperationException>(() => useCase.Execute(SoundboardId.New()));

        Assert.Equal("La soundboard demandee est introuvable.", exception.Message);
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