using Avalonia;
using Avalonia.Controls;

namespace YJCabin.Desktop.Views;

public partial class LoadingOverlay : UserControl
{
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<LoadingOverlay, string>(nameof(Message), "加载中…");

    public LoadingOverlay()
    {
        InitializeComponent();
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}
