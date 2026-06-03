using System.Configuration;
using System.Data;
using System.Windows;

namespace DMsound.UI.Wpf;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (s, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), "Erreur au démarrage");
            ex.Handled = true;
        };
        base.OnStartup(e);
    }
}
