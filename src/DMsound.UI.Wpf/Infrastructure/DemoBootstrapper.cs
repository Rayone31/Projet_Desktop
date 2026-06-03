using DMsound.Application.UseCases.AssignHotkey;
using DMsound.Application.UseCases.AnalyzeAudioFile;
using DMsound.Application.UseCases.GetSoundboardDetails;
using DMsound.Application.UseCases.ImportSounds;
using DMsound.Application.UseCases.GetSoundEditorDetails;
using DMsound.Application.UseCases.ListVisibleSoundboards;
using DMsound.Application.UseCases.ListAudioOutputDevices;
using DMsound.Application.UseCases.PreviewSoundSelection;
using DMsound.Application.UseCases.PlaySound;
using DMsound.Application.UseCases.PlaySoundByHotkey;
using DMsound.Application.UseCases.ResetSoundToOriginal;
using DMsound.Application.UseCases.SaveTrimmedSound;
using DMsound.Application.UseCases.StopSoundPlayback;
using DMsound.Application.UseCases.TrimSoundSelection;
using DMsound.Application.UseCases.RenameSoundboard;
using DMsound.Application.UseCases.RenameSound;
using DMsound.Application.UseCases.SelectAudioOutputDevice;
using DMsound.Infrastructure.Audio;
using DMsound.Infrastructure.Persistence;
using DMsound.UI.Wpf.Presentation;

namespace DMsound.UI.Wpf.Infrastructure;

internal static class DemoBootstrapper
{
    public static MainWindowViewModel CreateMainWindowViewModel()
    {
        var repository = FileSystemSoundboardRepository.Create();
        var audioLibraryStorage = new AudioLibraryStorage();
        var playbackService = new AudioPlaybackService();
        var listVisibleSoundboardsUseCase = new ListVisibleSoundboardsUseCase(repository);
        var getSoundboardDetailsUseCase = new GetSoundboardDetailsUseCase(repository);
        var importSoundsUseCase = new ImportSoundsUseCase(repository, audioLibraryStorage);
        var getSoundEditorDetailsUseCase = new GetSoundEditorDetailsUseCase(repository, playbackService);
        var analyzeAudioFileUseCase = new AnalyzeAudioFileUseCase(playbackService);
        var previewSoundSelectionUseCase = new PreviewSoundSelectionUseCase(repository, playbackService);
        var trimSoundSelectionUseCase = new TrimSoundSelectionUseCase(repository, playbackService);
        var resetSoundToOriginalUseCase = new ResetSoundToOriginalUseCase(repository);
        var saveTrimmedSoundUseCase = new SaveTrimmedSoundUseCase(repository);
        var stopSoundPlaybackUseCase = new StopSoundPlaybackUseCase(playbackService);
        var listAudioOutputDevicesUseCase = new ListAudioOutputDevicesUseCase(playbackService);
        var selectAudioOutputDeviceUseCase = new SelectAudioOutputDeviceUseCase(playbackService);
        var assignHotkeyUseCase = new AssignHotkeyUseCase(repository);
        var playSoundUseCase = new PlaySoundUseCase(repository, playbackService);
        var playSoundByHotkeyUseCase = new PlaySoundByHotkeyUseCase(repository, playbackService);
        var renameSoundboardUseCase = new RenameSoundboardUseCase(repository);
        var renameSoundUseCase = new RenameSoundUseCase(repository);

        var viewModel = new MainWindowViewModel(
            listVisibleSoundboardsUseCase,
            getSoundboardDetailsUseCase,
            importSoundsUseCase,
            getSoundEditorDetailsUseCase,
            analyzeAudioFileUseCase,
            previewSoundSelectionUseCase,
            trimSoundSelectionUseCase,
            resetSoundToOriginalUseCase,
            saveTrimmedSoundUseCase,
            stopSoundPlaybackUseCase,
            listAudioOutputDevicesUseCase,
            selectAudioOutputDeviceUseCase,
            assignHotkeyUseCase,
            playSoundUseCase,
            playSoundByHotkeyUseCase,
            renameSoundboardUseCase,
            renameSoundUseCase);

        viewModel.Load();
        return viewModel;
    }
}
