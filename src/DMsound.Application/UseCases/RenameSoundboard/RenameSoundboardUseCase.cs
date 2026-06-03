using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.RenameSoundboard
{
    public sealed class RenameSoundboardUseCase
    {
        private readonly ISoundboardRepository _repository;

        public RenameSoundboardUseCase(ISoundboardRepository repository)
        {
            _repository = repository;
        }

        public void Execute(SoundboardId soundboardId, string newName)
        {
            var soundboard = _repository.GetById(soundboardId)
                ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

            soundboard.Rename(newName);
            _repository.Update(soundboard);
        }
    }
}
