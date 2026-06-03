using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.RenameSound
{
    public sealed class RenameSoundUseCase
    {
        private readonly ISoundboardRepository _repository;

        public RenameSoundUseCase(ISoundboardRepository repository)
        {
            _repository = repository;
        }

        public void Execute(SoundboardId soundboardId, SoundId soundId, string newName)
        {
            var soundboard = _repository.GetById(soundboardId)
                ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

            var sound = soundboard.GetSoundById(soundId);
            sound.Rename(newName);
            _repository.Update(soundboard);
        }
    }
}
