using DMsound.Domain;
using System.Windows.Input;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class SoundItemViewModel : ObservableObject
{
    private string _name;
    private string _hotkeyDraft;
    private string _nameDraft;

    public SoundItemViewModel(
        SoundId id,
        string name,
        string? hotkey,
        ICommand playCommand,
        ICommand assignHotkeyCommand,
        ICommand selectCommand,
        ICommand renameSoundCommand)
    {
        Id = id;
        _name = name;
        _nameDraft = name;
        Hotkey = hotkey;
        PlayCommand = playCommand;
        AssignHotkeyCommand = assignHotkeyCommand;
        SelectCommand = selectCommand;
        RenameSoundCommand = renameSoundCommand;
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
