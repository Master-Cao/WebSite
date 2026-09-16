using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using YJCabin.Desktop.Services;
using YJCabin.Desktop.ViewModels;
using YJCabin.Desktop.Views;

namespace YJCabin.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var api = new ApiClient("http://localhost:5178");
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(api)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
