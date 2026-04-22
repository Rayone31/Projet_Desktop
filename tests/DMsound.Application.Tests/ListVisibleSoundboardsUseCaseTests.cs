using DMsound.Application.Abstractions;
using DMsound.Application.UseCases.ListVisibleSoundboards;
using DMsound.Domain;

namespace DMsound.Application.Tests;

public sealed class ListVisibleSoundboardsUseCaseTests
{
    [Fact]
    public void Execute_returns_only_visible_soundboards()
    {
        var repository = new FakeSoundboardRepository();
        repository.Add(new Soundboard(SoundboardId.New(), "Gaming"));

        var hidden = new Soundboard(SoundboardId.New(), "Montage");
        hidden.SetVisibility(false);
        repository.Add(hidden);

        var useCase = new ListVisibleSoundboardsUseCase(repository);

        var result = useCase.Execute();

        Assert.Single(result);
        Assert.Equal("Gaming", result[0].Name);
    }

    private sealed class FakeSoundboardRepository : ISoundboardRepository
    {
        private readonly List<Soundboard> _items = new();

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