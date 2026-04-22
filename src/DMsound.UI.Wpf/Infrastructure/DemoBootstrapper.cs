using DMsound.Application.UseCases.AssignHotkey;
using DMsound.Application.UseCases.GetSoundboardDetails;
using DMsound.Application.UseCases.ListVisibleSoundboards;
using DMsound.Application.UseCases.ListAudioOutputDevices;
using DMsound.Application.UseCases.PlaySound;
using DMsound.Application.UseCases.PlaySoundByHotkey;
using DMsound.Application.UseCases.SelectAudioOutputDevice;
using DMsound.Infrastructure.Audio;
using DMsound.UI.Wpf.Presentation;

namespace DMsound.UI.Wpf.Infrastructure;

internal static class DemoBootstrapper
{
    public static MainWindowViewModel CreateMainWindowViewModel()
    {
        var repository = DemoSoundboardRepository.Create();
        var playbackService = new AudioPlaybackService();
        var listVisibleSoundboardsUseCase = new ListVisibleSoundboardsUseCase(repository);
        var getSoundboardDetailsUseCase = new GetSoundboardDetailsUseCase(repository);
        var listAudioOutputDevicesUseCase = new ListAudioOutputDevicesUseCase(playbackService);
        var selectAudioOutputDeviceUseCase = new SelectAudioOutputDeviceUseCase(playbackService);
        var assignHotkeyUseCase = new AssignHotkeyUseCase(repository);
        var playSoundUseCase = new PlaySoundUseCase(repository, playbackService);
        var playSoundByHotkeyUseCase = new PlaySoundByHotkeyUseCase(repository, playbackService);

        var viewModel = new MainWindowViewModel(
            listVisibleSoundboardsUseCase,
            getSoundboardDetailsUseCase,
            listAudioOutputDevicesUseCase,
            selectAudioOutputDeviceUseCase,
            assignHotkeyUseCase,
            playSoundUseCase,
            playSoundByHotkeyUseCase);

        viewModel.Load();
        return viewModel;
    }
}