using DMsound.Domain;
using System.Windows.Input;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class SoundItemViewModel : ObservableObject
{
    private string _name;
    private string _hotkeyDraft;
    private string _nameDraft;

    private bool _isCapturing;
    private bool _isEnabled;

    public SoundItemViewModel(
        SoundId id,
        string name,
        string? hotkey,
        bool isEnabled,
        ICommand playCommand,
        ICommand assignHotkeyCommand,
        ICommand selectCommand,
        ICommand renameSoundCommand,
        ICommand startCapturingHotkeyCommand,
        ICommand toggleEnabledCommand)
    {
        Id = id;
        _name = name;
        _nameDraft = name;
        Hotkey = hotkey;
        _isEnabled = isEnabled;
        PlayCommand = playCommand;
        AssignHotkeyCommand = assignHotkeyCommand;
        SelectCommand = selectCommand;
        RenameSoundCommand = renameSoundCommand;
        StartCapturingHotkeyCommand = startCapturingHotkeyCommand;
        ToggleEnabledCommand = toggleEnabledCommand;
        _hotkeyDraft = hotkey ?? string.Empty;
    }

    public SoundId Id { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string NameDraft
    {
        get => _nameDraft;
        set => SetProperty(ref _nameDraft, value);
    }

    public string? Hotkey { get; private set; }

    public string HotkeyDraft
    {
        get => _hotkeyDraft;
        set => SetProperty(ref _hotkeyDraft, value);
    }

    public ICommand PlayCommand { get; }

    public ICommand AssignHotkeyCommand { get; }

    public ICommand SelectCommand { get; }

    public ICommand RenameSoundCommand { get; }

    public ICommand StartCapturingHotkeyCommand { get; }

    public ICommand ToggleEnabledCommand { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public bool IsCapturing
    {
        get => _isCapturing;
        set
        {
            if (SetProperty(ref _isCapturing, value))
            {
                HotkeyDraft = value ? "Appuyez..." : (Hotkey ?? string.Empty);
            }
        }
    }

    public void UpdateHotkey(string? hotkey)
    {
        Hotkey = hotkey;
        HotkeyDraft = hotkey ?? string.Empty;
        OnPropertyChanged(nameof(Hotkey));
    }

    public void UpdateName(string name)
    {
        Name = name;
        NameDraft = name;
    }
}
