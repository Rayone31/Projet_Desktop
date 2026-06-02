using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using DMsound.Session.LanTester;
using DMsound.UI.Wpf.Infrastructure;

namespace DMsound.UI.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Presentation.MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = DemoBootstrapper.CreateMainWindowViewModel();
        DataContext = _viewModel;
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
            Filter = "Fichiers audio|*.mp3;*.wav;*.wma;*.aac;*.flac;*.m4a;*.aiff|Tous les fichiers|*.*",
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _viewModel.ImportAudioFiles(dialog.FileNames);
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