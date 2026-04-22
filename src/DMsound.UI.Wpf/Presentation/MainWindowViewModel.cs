using System.Collections.ObjectModel;
using DMsound.Application.Models;
using DMsound.Application.UseCases.AssignHotkey;
using DMsound.Application.UseCases.GetSoundboardDetails;
using DMsound.Application.UseCases.ListAudioOutputDevices;
using DMsound.Application.UseCases.ListVisibleSoundboards;
using DMsound.Application.UseCases.PlaySound;
using DMsound.Application.UseCases.PlaySoundByHotkey;
using DMsound.Application.UseCases.SelectAudioOutputDevice;
using DMsound.Domain;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class MainWindowViewModel : ObservableObject
{
    private readonly ListVisibleSoundboardsUseCase _listVisibleSoundboardsUseCase;
    private readonly GetSoundboardDetailsUseCase _getSoundboardDetailsUseCase;
    private readonly ListAudioOutputDevicesUseCase _listAudioOutputDevicesUseCase;
    private readonly SelectAudioOutputDeviceUseCase _selectAudioOutputDeviceUseCase;
    private readonly AssignHotkeyUseCase _assignHotkeyUseCase;
    private readonly PlaySoundUseCase _playSoundUseCase;
    private readonly PlaySoundByHotkeyUseCase _playSoundByHotkeyUseCase;
    private SoundboardItemViewModel? _selectedSoundboard;
    private AudioOutputDeviceItemViewModel? _selectedAudioOutputDevice;
    private string _statusMessage = "Choisis une soundboard.";

    public MainWindowViewModel(
        ListVisibleSoundboardsUseCase listVisibleSoundboardsUseCase,
        GetSoundboardDetailsUseCase getSoundboardDetailsUseCase,
        ListAudioOutputDevicesUseCase listAudioOutputDevicesUseCase,
        SelectAudioOutputDeviceUseCase selectAudioOutputDeviceUseCase,
        AssignHotkeyUseCase assignHotkeyUseCase,
        PlaySoundUseCase playSoundUseCase,
        PlaySoundByHotkeyUseCase playSoundByHotkeyUseCase)
    {
        _listVisibleSoundboardsUseCase = listVisibleSoundboardsUseCase;
        _getSoundboardDetailsUseCase = getSoundboardDetailsUseCase;
        _listAudioOutputDevicesUseCase = listAudioOutputDevicesUseCase;
        _selectAudioOutputDeviceUseCase = selectAudioOutputDeviceUseCase;
        _assignHotkeyUseCase = assignHotkeyUseCase;
        _playSoundUseCase = playSoundUseCase;
        _playSoundByHotkeyUseCase = playSoundByHotkeyUseCase;
        Soundboards = new ObservableCollection<SoundboardItemViewModel>();
        Sounds = new ObservableCollection<SoundItemViewModel>();
        AudioOutputDevices = new ObservableCollection<AudioOutputDeviceItemViewModel>();
    }

    public ObservableCollection<SoundboardItemViewModel> Soundboards { get; }

    public ObservableCollection<SoundItemViewModel> Sounds { get; }

    public ObservableCollection<AudioOutputDeviceItemViewModel> AudioOutputDevices { get; }

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

    private void LoadSelectedSoundboard()
    {
        Sounds.Clear();

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
            new RelayCommand(() => AssignHotkey(soundboardId, sound.Id)));
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