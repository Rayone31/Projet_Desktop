using DMsound.Domain;

namespace DMsound.Application.Abstractions;

public interface ISoundboardRepository
{
    void Add(Soundboard soundboard);

    Soundboard? GetById(SoundboardId id);

    IReadOnlyCollection<Soundboard> GetAll();

    bool ExistsByName(string name);
}