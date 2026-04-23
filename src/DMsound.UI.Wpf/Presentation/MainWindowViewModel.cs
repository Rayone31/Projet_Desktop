using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using DMsound.Application.Models;
using DMsound.Application.UseCases.AnalyzeAudioFile;
using DMsound.Application.UseCases.AssignHotkey;
using DMsound.Application.UseCases.GetSoundboardDetails;
using DMsound.Application.UseCases.GetSoundEditorDetails;
using DMsound.Application.UseCases.ImportSounds;
using DMsound.Application.UseCases.ListAudioOutputDevices;
using DMsound.Application.UseCases.ListVisibleSoundboards;
using DMsound.Application.UseCases.PreviewSoundSelection;
using DMsound.Application.UseCases.PlaySound;
using DMsound.Application.UseCases.PlaySoundByHotkey;
using DMsound.Application.UseCases.ResetSoundToOriginal;
using DMsound.Application.UseCases.SaveTrimmedSound;
using DMsound.Application.UseCases.SelectAudioOutputDevice;
using DMsound.Application.UseCases.StopSoundPlayback;
using DMsound.Application.UseCases.TrimSoundSelection;
using DMsound.Domain;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class MainWindowViewModel : ObservableObject
{
    private const double WaveformCanvasWidthPixels = 480d;

    private readonly ListVisibleSoundboardsUseCase _listVisibleSoundboardsUseCase;
    private readonly GetSoundboardDetailsUseCase _getSoundboardDetailsUseCase;
    private readonly ImportSoundsUseCase _importSoundsUseCase;
    private readonly GetSoundEditorDetailsUseCase _getSoundEditorDetailsUseCase;
    private readonly AnalyzeAudioFileUseCase _analyzeAudioFileUseCase;
    private readonly PreviewSoundSelectionUseCase _previewSoundSelectionUseCase;
    private readonly TrimSoundSelectionUseCase _trimSoundSelectionUseCase;
    private readonly ResetSoundToOriginalUseCase _resetSoundToOriginalUseCase;
    private readonly SaveTrimmedSoundUseCase _saveTrimmedSoundUseCase;
    private readonly StopSoundPlaybackUseCase _stopSoundPlaybackUseCase;
    private readonly ListAudioOutputDevicesUseCase _listAudioOutputDevicesUseCase;
    private readonly SelectAudioOutputDeviceUseCase _selectAudioOutputDeviceUseCase;
    private readonly AssignHotkeyUseCase _assignHotkeyUseCase;
    private readonly PlaySoundUseCase _playSoundUseCase;
    private readonly PlaySoundByHotkeyUseCase _playSoundByHotkeyUseCase;

    private SoundboardItemViewModel? _selectedSoundboard;
    private AudioOutputDeviceItemViewModel? _selectedAudioOutputDevice;
    private SoundEditorDetails? _selectedSoundEditor;
    private SoundId? _editingSoundId;
    private string? _pendingTrimmedFilePath;

    private double _originalDurationSeconds = 1d;
    private double _editableDurationSeconds = 1d;
    private double _originalMarkerSeconds;
    private double _editableMarkerSeconds;
    private double _selectionStartSeconds;
    private double _selectionEndSeconds = 1d;

    private string _editableFilePath = string.Empty;
    private Visibility _editorVisibility = Visibility.Collapsed;
    private string _statusMessage = "Choisis une soundboard.";

    private readonly DispatcherTimer _playbackCursorTimer;
    private DateTime _playbackCursorStartUtc;
    private double _playbackCursorStartSeconds;
    private double _playbackCursorEndSeconds;
    private bool _playbackCursorForOriginalWaveform;

    public MainWindowViewModel(
        ListVisibleSoundboardsUseCase listVisibleSoundboardsUseCase,
        GetSoundboardDetailsUseCase getSoundboardDetailsUseCase,
        ImportSoundsUseCase importSoundsUseCase,
        GetSoundEditorDetailsUseCase getSoundEditorDetailsUseCase,
        AnalyzeAudioFileUseCase analyzeAudioFileUseCase,
        PreviewSoundSelectionUseCase previewSoundSelectionUseCase,
        TrimSoundSelectionUseCase trimSoundSelectionUseCase,
        ResetSoundToOriginalUseCase resetSoundToOriginalUseCase,
        SaveTrimmedSoundUseCase saveTrimmedSoundUseCase,
        StopSoundPlaybackUseCase stopSoundPlaybackUseCase,
        ListAudioOutputDevicesUseCase listAudioOutputDevicesUseCase,
        SelectAudioOutputDeviceUseCase selectAudioOutputDeviceUseCase,
        AssignHotkeyUseCase assignHotkeyUseCase,
        PlaySoundUseCase playSoundUseCase,
        PlaySoundByHotkeyUseCase playSoundByHotkeyUseCase)
    {
        _listVisibleSoundboardsUseCase = listVisibleSoundboardsUseCase;
        _getSoundboardDetailsUseCase = getSoundboardDetailsUseCase;
        _importSoundsUseCase = importSoundsUseCase;
        _getSoundEditorDetailsUseCase = getSoundEditorDetailsUseCase;
        _analyzeAudioFileUseCase = analyzeAudioFileUseCase;
        _previewSoundSelectionUseCase = previewSoundSelectionUseCase;
        _trimSoundSelectionUseCase = trimSoundSelectionUseCase;
        _resetSoundToOriginalUseCase = resetSoundToOriginalUseCase;
        _saveTrimmedSoundUseCase = saveTrimmedSoundUseCase;
        _stopSoundPlaybackUseCase = stopSoundPlaybackUseCase;
        _listAudioOutputDevicesUseCase = listAudioOutputDevicesUseCase;
        _selectAudioOutputDeviceUseCase = selectAudioOutputDeviceUseCase;
        _assignHotkeyUseCase = assignHotkeyUseCase;
        _playSoundUseCase = playSoundUseCase;
        _playSoundByHotkeyUseCase = playSoundByHotkeyUseCase;

        Soundboards = new ObservableCollection<SoundboardItemViewModel>();
        Sounds = new ObservableCollection<SoundItemViewModel>();
        AudioOutputDevices = new ObservableCollection<AudioOutputDeviceItemViewModel>();
        OriginalWaveformPeaks = new ObservableCollection<double>();
        EditableWaveformPeaks = new ObservableCollection<double>();

        PlayOriginalCommand = new RelayCommand(PlayOriginalWaveform);
        PlayEditedCommand = new RelayCommand(PlayEditedWaveform);
        StopPlaybackCommand = new RelayCommand(StopPlayback);
        ResetOriginalMarkerCommand = new RelayCommand(() => OriginalMarkerSeconds = 0d);
        ResetEditedMarkerCommand = new RelayCommand(() => EditableMarkerSeconds = SelectionStartSeconds);
        TrimSelectionCommand = new RelayCommand(TrimEditedWaveform);
        ResetEditedSoundCommand = new RelayCommand(ResetEditedSound);
        SaveTrimmedCommand = new RelayCommand(SaveTrimmedWaveform);
        CloseEditorCommand = new RelayCommand(CloseEditor);

        _playbackCursorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50),
        };
        _playbackCursorTimer.Tick += HandlePlaybackCursorTick;
    }

    public ObservableCollection<SoundboardItemViewModel> Soundboards { get; }

    public ObservableCollection<SoundItemViewModel> Sounds { get; }

    public ObservableCollection<AudioOutputDeviceItemViewModel> AudioOutputDevices { get; }

    public ObservableCollection<double> OriginalWaveformPeaks { get; }

    public ObservableCollection<double> EditableWaveformPeaks { get; }

    public RelayCommand PlayOriginalCommand { get; }

    public RelayCommand PlayEditedCommand { get; }

    public RelayCommand StopPlaybackCommand { get; }

    public RelayCommand ResetOriginalMarkerCommand { get; }

    public RelayCommand ResetEditedMarkerCommand { get; }

    public RelayCommand TrimSelectionCommand { get; }

    public RelayCommand ResetEditedSoundCommand { get; }

    public RelayCommand SaveTrimmedCommand { get; }

    public RelayCommand CloseEditorCommand { get; }

    public double WaveformCanvasWidthPixelsValue => WaveformCanvasWidthPixels;

    public double OriginalWaveformCursorX => BuildWaveformCursorX(OriginalMarkerSeconds, OriginalDurationSeconds);

    public double EditableWaveformCursorX => BuildWaveformCursorX(EditableMarkerSeconds, EditableDurationSeconds);

    public double EditableSelectionStartCursorX => BuildWaveformCursorX(SelectionStartSeconds, EditableDurationSeconds);

    public double EditableSelectionEndCursorX => BuildWaveformCursorX(SelectionEndSeconds, EditableDurationSeconds);

    public SoundboardItemViewModel? SelectedSoundboard
    {
        get => _selectedSoundboard;
        set
        {
            if (SetProperty(ref _selectedSoundboard, value))
            {
                LoadSelectedSoundboard();
            }
        }
    }

    public AudioOutputDeviceItemViewModel? SelectedAudioOutputDevice
    {
        get => _selectedAudioOutputDevice;
        set
        {
            if (SetProperty(ref _selectedAudioOutputDevice, value) && value is not null)
            {
                _selectAudioOutputDeviceUseCase.Execute(value.DeviceNumber);
                StatusMessage = $"Sortie audio selectionnee: {value.Name}.";
            }
        }
    }

    public SoundEditorDetails? SelectedSoundEditor
    {
        get => _selectedSoundEditor;
        private set => SetProperty(ref _selectedSoundEditor, value);
    }

    public Visibility EditorVisibility
    {
        get => _editorVisibility;
        private set => SetProperty(ref _editorVisibility, value);
    }

    public string EditableFilePath
    {
        get => _editableFilePath;
        private set => SetProperty(ref _editableFilePath, value);
    }

    public double OriginalDurationSeconds
    {
        get => _originalDurationSeconds;
        private set
        {
            if (SetProperty(ref _originalDurationSeconds, Math.Max(0.1d, value)))
            {
                OnPropertyChanged(nameof(OriginalWaveformCursorX));
            }
        }
    }

    public double EditableDurationSeconds
    {
        get => _editableDurationSeconds;
        private set
        {
            if (SetProperty(ref _editableDurationSeconds, Math.Max(0.1d, value)))
            {
                OnPropertyChanged(nameof(EditableWaveformCursorX));
                OnPropertyChanged(nameof(EditableSelectionStartCursorX));
                OnPropertyChanged(nameof(EditableSelectionEndCursorX));
            }
        }
    }

    public double OriginalMarkerSeconds
    {
        get => _originalMarkerSeconds;
        set
        {
            if (SetProperty(ref _originalMarkerSeconds, Math.Clamp(value, 0d, OriginalDurationSeconds)))
            {
                OnPropertyChanged(nameof(OriginalWaveformCursorX));
            }
        }
    }

    public double EditableMarkerSeconds
    {
        get => _editableMarkerSeconds;
        set
        {
            if (SetProperty(ref _editableMarkerSeconds, Math.Clamp(value, 0d, EditableDurationSeconds)))
            {
                OnPropertyChanged(nameof(EditableWaveformCursorX));
            }
        }
    }

    public double SelectionStartSeconds
    {
        get => _selectionStartSeconds;
        set
        {
            var start = Math.Clamp(value, 0d, EditableDurationSeconds);

            if (!SetProperty(ref _selectionStartSeconds, start))
            {
                return;
            }

            OnPropertyChanged(nameof(EditableSelectionStartCursorX));

            if (SelectionEndSeconds <= start)
            {
                SelectionEndSeconds = Math.Min(EditableDurationSeconds, start + 0.1d);
            }

            if (EditableMarkerSeconds < start)
            {
                EditableMarkerSeconds = start;
            }
        }
    }

    public double SelectionEndSeconds
    {
        get => _selectionEndSeconds;
        set
        {
            var end = Math.Clamp(value, 0d, EditableDurationSeconds);

            if (SetProperty(ref _selectionEndSeconds, end))
            {
                OnPropertyChanged(nameof(EditableSelectionEndCursorX));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public void Load()
    {
        LoadAudioOutputDevices();
        Soundboards.Clear();

        foreach (var soundboard in _listVisibleSoundboardsUseCase.Execute())
        {
            Soundboards.Add(new SoundboardItemViewModel(soundboard.Id, soundboard.Name));
        }

        SelectedSoundboard = Soundboards.FirstOrDefault();

        if (SelectedSoundboard is null)
        {
            StatusMessage = "Aucune soundboard visible n'est disponible.";
        }
    }

    public void HandleKeyPress(string keyText)
    {
        if (SelectedSoundboard is null)
        {
            return;
        }

        try
        {
            var played = _playSoundByHotkeyUseCase.Execute(SelectedSoundboard.Id, keyText);

            if (played)
            {
                StatusMessage = $"Lecture declenchee par la touche {keyText.ToUpperInvariant()}.";
            }
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    public void ImportAudioFiles(IEnumerable<string> filePaths)
    {
        if (SelectedSoundboard is null)
        {
            StatusMessage = "Choisis d'abord une soundboard pour importer des fichiers audio.";
            return;
        }

        try
        {
            var importedSounds = _importSoundsUseCase.Execute(SelectedSoundboard.Id, filePaths);
            LoadSelectedSoundboard();
            StatusMessage = $"{importedSounds.Count} fichier(s) audio importe(s) dans '{SelectedSoundboard.Name}'.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void LoadSelectedSoundboard()
    {
        Sounds.Clear();
        CloseEditor();

        if (SelectedSoundboard is null)
        {
            return;
        }

        var details = _getSoundboardDetailsUseCase.Execute(SelectedSoundboard.Id);

        foreach (var sound in details.Sounds)
        {
            Sounds.Add(CreateSoundItemViewModel(details.Id, sound));
        }

        StatusMessage = $"Soundboard '{details.Name}' chargee.";
    }

    private SoundItemViewModel CreateSoundItemViewModel(SoundboardId soundboardId, SoundSummary sound)
    {
        return new SoundItemViewModel(
            sound.Id,
            sound.Name,
            sound.Hotkey?.Value,
            new RelayCommand(() => PlaySound(soundboardId, sound)),
            new RelayCommand(() => AssignHotkey(soundboardId, sound.Id)),
            new RelayCommand(() => SelectSoundForEditing(soundboardId, sound.Id)));
    }

    private void SelectSoundForEditing(SoundboardId soundboardId, SoundId soundId)
    {
        try
        {
            var details = _getSoundEditorDetailsUseCase.Execute(soundboardId, soundId);
            _editingSoundId = soundId;
            _pendingTrimmedFilePath = null;
            SelectedSoundEditor = details;

            EditorVisibility = Visibility.Visible;
            EditableFilePath = details.FilePath;
            OriginalDurationSeconds = details.DurationSeconds;
            EditableDurationSeconds = details.DurationSeconds;

            OriginalMarkerSeconds = 0d;
            SelectionStartSeconds = 0d;
            SelectionEndSeconds = EditableDurationSeconds;
            EditableMarkerSeconds = 0d;

            FillWaveform(OriginalWaveformPeaks, details.WaveformPeaks);
            FillWaveform(EditableWaveformPeaks, details.WaveformPeaks);
            StatusMessage = $"Editeur charge pour '{details.Name}'.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void ResetEditedSound()
    {
        if (!CanUseEditor())
        {
            return;
        }

        try
        {
            _stopSoundPlaybackUseCase.Execute();
            _resetSoundToOriginalUseCase.Execute(
                SelectedSoundboard!.Id,
                _editingSoundId!.Value,
                _pendingTrimmedFilePath ?? SelectedSoundEditor?.FilePath);
            _pendingTrimmedFilePath = null;

            SelectSoundForEditing(SelectedSoundboard.Id, _editingSoundId.Value);
            StatusMessage = "La version originale a ete restauree.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void PlayOriginalWaveform()
    {
        if (!CanUseEditor())
        {
            return;
        }

        try
        {
            _previewSoundSelectionUseCase.Execute(
                SelectedSoundboard!.Id,
                _editingSoundId!.Value,
                OriginalMarkerSeconds,
                OriginalDurationSeconds);

            StartPlaybackCursorTracking(isOriginalWaveform: true, OriginalMarkerSeconds, OriginalDurationSeconds);

            StatusMessage = "Lecture de l'onde originale.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void PlayEditedWaveform()
    {
        if (!CanUseEditor())
        {
            return;
        }

        if (!TryBuildEditableSelection(out var start, out var end))
        {
            return;
        }

        try
        {
            _previewSoundSelectionUseCase.Execute(
                SelectedSoundboard!.Id,
                _editingSoundId!.Value,
                start,
                end);

            StartPlaybackCursorTracking(isOriginalWaveform: false, start, end);

            StatusMessage = "Lecture de l'onde modifiee.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void StopPlayback()
    {
        _stopSoundPlaybackUseCase.Execute();
        _playbackCursorTimer.Stop();
        StatusMessage = "Lecture arretee.";
    }

    private void TrimEditedWaveform()
    {
        if (!CanUseEditor())
        {
            return;
        }

        if (!TryBuildEditableSelection(out var start, out var end))
        {
            return;
        }

        try
        {
            var trimmedPath = _trimSoundSelectionUseCase.Execute(
                SelectedSoundboard!.Id,
                _editingSoundId!.Value,
                start,
                end);

            _pendingTrimmedFilePath = trimmedPath;
            var analysis = _analyzeAudioFileUseCase.Execute(trimmedPath);

            EditableFilePath = trimmedPath;
            EditableDurationSeconds = analysis.DurationSeconds;
            SelectionStartSeconds = 0d;
            SelectionEndSeconds = EditableDurationSeconds;
            EditableMarkerSeconds = 0d;
            FillWaveform(EditableWaveformPeaks, analysis.WaveformPeaks);

            StatusMessage = "Decoupe prete. Clique sur Sauvegarder pour l'appliquer au menu.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void SaveTrimmedWaveform()
    {
        if (!CanUseEditor())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_pendingTrimmedFilePath))
        {
            StatusMessage = "Aucune nouvelle decoupe a sauvegarder.";
            return;
        }

        try
        {
            _saveTrimmedSoundUseCase.Execute(SelectedSoundboard!.Id, _editingSoundId!.Value, _pendingTrimmedFilePath);
            _pendingTrimmedFilePath = null;

            SelectSoundForEditing(SelectedSoundboard.Id, _editingSoundId.Value);
            StatusMessage = "Version decoupee sauvegardee. Le menu jouera cette version.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void CloseEditor()
    {
        _playbackCursorTimer.Stop();

        EditorVisibility = Visibility.Collapsed;
        SelectedSoundEditor = null;
        _editingSoundId = null;
        _pendingTrimmedFilePath = null;
        EditableFilePath = string.Empty;

        OriginalWaveformPeaks.Clear();
        EditableWaveformPeaks.Clear();
    }

    private bool CanUseEditor()
    {
        if (SelectedSoundboard is not null && _editingSoundId.HasValue && SelectedSoundEditor is not null)
        {
            return true;
        }

        StatusMessage = "Clique sur Editer pour ouvrir l'editeur audio.";
        return false;
    }

    private bool TryBuildEditableSelection(out double start, out double end)
    {
        start = Math.Max(SelectionStartSeconds, EditableMarkerSeconds);
        end = SelectionEndSeconds;

        if (end > start)
        {
            return true;
        }

        StatusMessage = "La plage de decoupe est invalide.";
        return false;
    }

    private static void FillWaveform(ObservableCollection<double> target, IReadOnlyList<double> peaks)
    {
        target.Clear();

        foreach (var peak in peaks)
        {
            target.Add(peak);
        }
    }

    private void StartPlaybackCursorTracking(bool isOriginalWaveform, double startSeconds, double endSeconds)
    {
        _playbackCursorForOriginalWaveform = isOriginalWaveform;
        _playbackCursorStartSeconds = startSeconds;
        _playbackCursorEndSeconds = endSeconds;
        _playbackCursorStartUtc = DateTime.UtcNow;
        _playbackCursorTimer.Start();
    }

    private void HandlePlaybackCursorTick(object? sender, EventArgs eventArgs)
    {
        var elapsed = (DateTime.UtcNow - _playbackCursorStartUtc).TotalSeconds;
        var current = Math.Min(_playbackCursorStartSeconds + elapsed, _playbackCursorEndSeconds);

        if (_playbackCursorForOriginalWaveform)
        {
            OriginalMarkerSeconds = current;
        }
        else
        {
            EditableMarkerSeconds = current;
        }

        if (current >= _playbackCursorEndSeconds)
        {
            _playbackCursorTimer.Stop();
        }
    }

    private static double BuildWaveformCursorX(double markerSeconds, double durationSeconds)
    {
        var safeDuration = Math.Max(0.1d, durationSeconds);
        var progress = Math.Clamp(markerSeconds / safeDuration, 0d, 1d);
        return progress * (WaveformCanvasWidthPixels - 2d);
    }

    private void LoadAudioOutputDevices()
    {
        AudioOutputDevices.Clear();

        foreach (var device in _listAudioOutputDevicesUseCase.Execute())
        {
            AudioOutputDevices.Add(new AudioOutputDeviceItemViewModel(device.DeviceNumber, device.Name));
        }

        SelectedAudioOutputDevice = AudioOutputDevices.FirstOrDefault();
    }

    private void AssignHotkey(SoundboardId soundboardId, SoundId soundId)
    {
        var sound = Sounds.First(item => item.Id == soundId);

        try
        {
            _assignHotkeyUseCase.Execute(soundboardId, soundId, sound.HotkeyDraft);
            sound.UpdateHotkey(sound.HotkeyDraft.ToUpperInvariant());
            StatusMessage = $"Touche assignee a '{sound.Name}': {sound.Hotkey}.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }

    private void PlaySound(SoundboardId soundboardId, SoundSummary sound)
    {
        try
        {
            _playSoundUseCase.Execute(soundboardId, sound.Id);
            StatusMessage = $"Lecture de '{sound.Name}'.";
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
    }
}
