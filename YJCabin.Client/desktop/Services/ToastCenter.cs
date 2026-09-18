using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YJCabin.Desktop.Services;

public enum ToastKind
{
    Success,
    Info,
    Warning,
    Error
}

public sealed class ToastItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string Text { get; init; }
    public ToastKind Kind { get; init; }
    public bool IsSuccess => Kind == ToastKind.Success;
    public bool IsInfo => Kind == ToastKind.Info;
    public bool IsWarning => Kind == ToastKind.Warning;
    public bool IsError => Kind == ToastKind.Error;
}

public sealed class ToastCenter : ObservableObject
{
    public static ToastCenter Current { get; } = new();

    public ObservableCollection<ToastItem> Items { get; } = [];

    public void Success(string text) => Show(text, ToastKind.Success);

    public void Info(string text) => Show(text, ToastKind.Info);

    public void Warning(string text) => Show(text, ToastKind.Warning);

    public void Error(string text) => Show(text, ToastKind.Error);

    public void Show(string text, ToastKind kind)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var item = new ToastItem { Text = text.Trim(), Kind = kind };
        var lifetime = kind switch
        {
            ToastKind.Error => 5200,
            ToastKind.Warning => 4200,
            _ => 3200
        };

        Dispatcher.UIThread.Post(() =>
        {
            Items.Insert(0, item);
            while (Items.Count > 4)
            {
                Items.RemoveAt(Items.Count - 1);
            }
        });

        _ = DismissLaterAsync(item, lifetime);
    }

    public void Dismiss(ToastItem? item)
    {
        if (item is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => Items.Remove(item));
    }

    private async Task DismissLaterAsync(ToastItem item, int milliseconds)
    {
        await Task.Delay(milliseconds);
        Dismiss(item);
    }
}
