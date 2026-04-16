using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;
using Watchfolioo.Views; 

namespace Watchfolioo;

public partial class App : Application
{
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(); 
        }
        base.OnFrameworkInitializationCompleted();
    }
}