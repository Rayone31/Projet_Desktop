using DMsound.Domain;

namespace DMsound.UI.Wpf.Presentation;

internal sealed class SoundboardItemViewModel : ObservableObject
{
    private string _name;

    public SoundboardItemViewModel(SoundboardId id, string name)
    {
        Id = id;
        _name = name;
    }

    public SoundboardId Id { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}