using Avalonia.Controls;
using Avalonia.Input;
using YJCabin.Desktop.ViewModels;

namespace YJCabin.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) => WindowState = WindowState.Maximized;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape &&
            DataContext is MainWindowViewModel vm &&
            vm.IsImmersiveWriting)
        {
            vm.Articles.ExitImmersive();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}
