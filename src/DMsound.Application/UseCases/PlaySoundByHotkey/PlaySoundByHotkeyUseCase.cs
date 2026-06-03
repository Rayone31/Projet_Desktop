using DMsound.Application.Abstractions;
using DMsound.Domain;

namespace DMsound.Application.UseCases.PlaySoundByHotkey;

public sealed class PlaySoundByHotkeyUseCase
{
    private readonly ISoundboardRepository _repository;
    private readonly ISoundPlaybackService _playbackService;

    public PlaySoundByHotkeyUseCase(ISoundboardRepository repository, ISoundPlaybackService playbackService)
    {
        _repository = repository;
        _playbackService = playbackService;
    }

    public bool Execute(SoundboardId soundboardId, string hotkey)
    {
        var soundboard = _repository.GetById(soundboardId)
            ?? throw new InvalidOperationException("La soundboard demandee est introuvable.");

        var key = new Hotkey(hotkey);
        var sound = soundboard.FindSoundByHotkey(key);

        if (sound is null)
        {
            return false;
        }

        if (!sound.IsEnabled)
        {
            return false;
        }

        _playbackService.Play(sound.ModifiedFilePath, muteMicDuringPlayback: true);
        return true;
    }
}
