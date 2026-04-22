using DMsound.Domain;
using System.Windows.Input;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class SoundItemViewModel : ObservableObject
{
    private string _hotkeyDraft;

    public SoundItemViewModel(SoundId id, string name, string? hotkey, ICommand playCommand, ICommand assignHotkeyCommand)
    {
        Id = id;
        Name = name;
        Hotkey = hotkey;
        PlayCommand = playCommand;
        AssignHotkeyCommand = assignHotkeyCommand;
        _hotkeyDraft = hotkey ?? string.Empty;
    }

    public SoundId Id { get; }

    public string Name { get; }

    public string? Hotkey { get; private set; }

    public string HotkeyDraft
    {
        get => _hotkeyDraft;
        set => SetProperty(ref _hotkeyDraft, value);
    }

    public ICommand PlayCommand { get; }

    public ICommand AssignHotkeyCommand { get; }

    public void UpdateHotkey(string? hotkey)
    {
        Hotkey = hotkey;
        HotkeyDraft = hotkey ?? string.Empty;
        OnPropertyChanged(nameof(Hotkey));
    }
}