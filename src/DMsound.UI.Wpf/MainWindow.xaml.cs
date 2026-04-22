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
}