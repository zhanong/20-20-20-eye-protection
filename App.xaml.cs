using System.Windows;

namespace Eye202020;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// Override OnStartup to manually initialize MainWindow without showing it
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Create and initialize MainWindow but don't show it
        var mainWindow = new MainWindow();
        mainWindow.InitializeComponent();
        MainWindow = mainWindow;
    }
}

