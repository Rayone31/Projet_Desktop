using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using DMsound.UI.Wpf.Infrastructure;

namespace DMsound.UI.Wpf;

public partial class MainWindow : Window
{
    private readonly Presentation.MainWindowViewModel _viewModel;
    private readonly GlobalHotkeyService _globalHotkeyService;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = DemoBootstrapper.CreateMainWindowViewModel();
        DataContext = _viewModel;

        _globalHotkeyService = new GlobalHotkeyService();
        _globalHotkeyService.HotkeyPressed += OnGlobalHotkeyPressed;

        _viewModel.HotkeyRegistrationRequested += OnHotkeyRegistrationRequested;
        _viewModel.HotkeyUnregistrationRequested += OnHotkeyUnregistrationRequested;

        Loaded += OnWindowLoaded;
        Closed += OnWindowClosed;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        _globalHotkeyService.Attach(this);
        _viewModel.RegisterAllHotkeys();
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _globalHotkeyService.Dispose();
    }

    private void OnGlobalHotkeyPressed(string keyText)
    {
        _viewModel.HandleKeyPress(keyText);
    }

    private void OnHotkeyRegistrationRequested(string keyText)
    {
        _globalHotkeyService.Register(keyText);
    }

    private void OnHotkeyUnregistrationRequested(string keyText)
    {
        _globalHotkeyService.Unregister(keyText);
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        _viewModel.HandleKeyPress(e.Key.ToString());
    }

    private void OnWindowDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] filePaths || filePaths.Length == 0)
        {
            return;
        }

        _viewModel.ImportAudioFiles(filePaths);
    }

    private void OnImportAudioClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Importer des fichiers audio",
            Multiselect = true,
            Filter = "Fichiers audio (*.mp3;*.wav)|*.mp3;*.wav;*.wma;*.aac;*.flac;*.m4a;*.aiff|Tous les fichiers (*.*)|*.*",
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _viewModel.ImportAudioFiles(dialog.FileNames);
    }

    private void OnRenameSoundboardClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.RenameSelectedSoundboard();
    }

    private void OnRenameSoundClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.RenameActiveSound(_viewModel.ActiveSoundNameDraft);
    }

    private void OnOpenSessionClicked(object sender, RoutedEventArgs e)
    {
        var sessionWindow = new DMsound.Session.LanTester.MainWindow
        {
            Owner = this
        };
        sessionWindow.Show();
    }
}
