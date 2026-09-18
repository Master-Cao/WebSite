using Avalonia.Controls;
using Avalonia.Interactivity;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.Views;

public partial class ToastHost : UserControl
{
    public ToastHost()
    {
        InitializeComponent();
        DataContext = ToastCenter.Current;
    }

    private void OnDismiss(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ToastItem item })
        {
            ToastCenter.Current.Dismiss(item);
        }
    }
}
