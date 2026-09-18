using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using YJCabin.Desktop.Services;
using YJCabin.Desktop.ViewModels;

namespace YJCabin.Desktop.Views;

public partial class ArticlesPage : UserControl
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp"
    };

    private static readonly string[] PreferredImageFormats =
    [
        "PNG", "image/png", "image/jpeg", "image/jpg", "image/webp", "JFIF", "Bitmap"
    ];

    public ArticlesPage()
    {
        InitializeComponent();
    }

    private void OnPageKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || DataContext is not ArticlesViewModel vm || !vm.IsImmersive)
        {
            return;
        }

        vm.ExitImmersive();
        e.Handled = true;
    }

    private void OnMarkdownPasting(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        e.Handled = true;
        _ = PasteAsync(box);
    }

    private void OnMarkdownDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = HasImageFiles(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnMarkdownDrop(object? sender, DragEventArgs e)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        e.Handled = true;
        _ = DropAsync(box, e.Data);
    }

    private async void OnInsertImageClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ArticlesViewModel vm)
        {
            return;
        }

        var top = TopLevel.GetTopLevel(this);
        if (top is null)
        {
            return;
        }

        try
        {
            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择图片",
                AllowMultiple = true,
                FileTypeFilter =
                [
                    new FilePickerFileType("图片")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"]
                    }
                ]
            });

            foreach (var file in files)
            {
                await InsertStorageFileAsync(vm, MarkdownBox, file);
            }
        }
        catch (Exception ex)
        {
            ToastCenter.Current.Error(ex.Message);
        }
    }

    private async Task PasteAsync(TextBox box)
    {
        if (DataContext is not ArticlesViewModel vm)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        try
        {
            var formats = await clipboard.GetFormatsAsync();
            if (await TryInsertClipboardImageAsync(vm, box, clipboard, formats))
            {
                return;
            }

            var text = await clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                InsertText(box, text);
            }
        }
        catch (Exception ex)
        {
            ToastCenter.Current.Error(ex.Message);
        }
    }

    private async Task DropAsync(TextBox box, IDataObject data)
    {
        if (DataContext is not ArticlesViewModel vm)
        {
            return;
        }

        try
        {
            var files = data.GetFiles()?.OfType<IStorageFile>().Where(IsImageFile).ToList() ?? [];
            foreach (var file in files)
            {
                await InsertStorageFileAsync(vm, box, file);
            }
        }
        catch (Exception ex)
        {
            ToastCenter.Current.Error(ex.Message);
        }
    }

    private static async Task InsertStorageFileAsync(ArticlesViewModel vm, TextBox box, IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        copy.Position = 0;
        var caret = await vm.InsertImageAsync(box.CaretIndex, copy, file.Name, ContentTypeOf(file.Name));
        box.CaretIndex = caret;
    }

    private static async Task<bool> TryInsertClipboardImageAsync(
        ArticlesViewModel vm,
        TextBox box,
        Avalonia.Input.Platform.IClipboard clipboard,
        IEnumerable<string> formats)
    {
        var list = formats.ToArray();
        foreach (var format in PreferredImageFormats.Concat(list.Where(LooksLikeImageFormat)))
        {
            if (!list.Contains(format, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var raw = await clipboard.GetDataAsync(format);
            if (await TryInsertRawAsync(vm, box, raw, format))
            {
                return true;
            }
        }

        if (list.Contains(DataFormats.Files, StringComparer.OrdinalIgnoreCase)
            && await clipboard.GetDataAsync(DataFormats.Files) is IEnumerable<IStorageItem> items)
        {
            var inserted = false;
            foreach (var file in items.OfType<IStorageFile>().Where(IsImageFile))
            {
                await InsertStorageFileAsync(vm, box, file);
                inserted = true;
            }

            return inserted;
        }

        foreach (var format in list.Where(format => !IsTextLikeFormat(format)))
        {
            var raw = await clipboard.GetDataAsync(format);
            if (await TryInsertRawAsync(vm, box, raw, format))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> TryInsertRawAsync(ArticlesViewModel vm, TextBox box, object? raw, string format)
    {
        var bytes = await ToBytesAsync(raw);
        var png = NormalizeImage(bytes);
        if (png is not { Length: > 0 })
        {
            return false;
        }

        using var stream = new MemoryStream(png);
        var (name, type) = NameAndType(format, png);
        var caret = await vm.InsertImageAsync(box.CaretIndex, stream, name, type);
        box.CaretIndex = caret;
        return true;
    }

    private static async Task<byte[]?> ToBytesAsync(object? raw)
    {
        switch (raw)
        {
            case byte[] bytes:
                return bytes;
            case MemoryStream memory:
                return memory.ToArray();
            case Stream stream:
                using (stream)
                {
                    using var copy = new MemoryStream();
                    await stream.CopyToAsync(copy);
                    return copy.ToArray();
                }
            case Bitmap bitmap:
                using (var png = new MemoryStream())
                {
                    bitmap.Save(png);
                    return png.ToArray();
                }
            default:
                return null;
        }
    }

    private static byte[]? NormalizeImage(byte[]? bytes)
    {
        if (bytes is not { Length: > 8 })
        {
            return null;
        }

        if (IsPng(bytes) || IsJpeg(bytes) || IsWebp(bytes))
        {
            return bytes;
        }

        if (TryDecodeBitmap(bytes, out var encoded))
        {
            return encoded;
        }

        var bmp = WrapDibAsBmp(bytes);
        return bmp is not null && TryDecodeBitmap(bmp, out encoded) ? encoded : null;
    }

    private static bool TryDecodeBitmap(byte[] bytes, out byte[]? png)
    {
        try
        {
            using var input = new MemoryStream(bytes);
            using var bitmap = new Bitmap(input);
            using var output = new MemoryStream();
            bitmap.Save(output);
            png = output.ToArray();
            return png.Length > 0;
        }
        catch
        {
            png = null;
            return false;
        }
    }

    private static byte[]? WrapDibAsBmp(byte[] dib)
    {
        if (dib.Length < 40)
        {
            return null;
        }

        var headerSize = BitConverter.ToInt32(dib, 0);
        if (headerSize is < 12 or > 256)
        {
            return null;
        }

        var bitCount = dib.Length > 14 ? BitConverter.ToInt16(dib, 14) : (short)0;
        var colorsUsed = dib.Length > 36 ? BitConverter.ToInt32(dib, 32) : 0;
        var palette = 0;
        if (bitCount is > 0 and <= 8)
        {
            palette = (colorsUsed > 0 ? colorsUsed : 1 << bitCount) * 4;
        }

        var file = new byte[14 + dib.Length];
        file[0] = (byte)'B';
        file[1] = (byte)'M';
        BitConverter.GetBytes(file.Length).CopyTo(file, 2);
        BitConverter.GetBytes(14 + headerSize + palette).CopyTo(file, 10);
        Buffer.BlockCopy(dib, 0, file, 14, dib.Length);
        return file;
    }

    private static void InsertText(TextBox box, string text)
    {
        var start = Math.Clamp(box.CaretIndex, 0, box.Text?.Length ?? 0);
        var current = box.Text ?? "";
        box.Text = current.Insert(start, text);
        box.CaretIndex = start + text.Length;
    }

    private static bool HasImageFiles(IDataObject data)
    {
        var files = data.GetFiles();
        return files?.OfType<IStorageFile>().Any(IsImageFile) == true;
    }

    private static bool IsImageFile(IStorageFile file) =>
        ImageExtensions.Contains(Path.GetExtension(file.Name));

    private static bool LooksLikeImageFormat(string format) =>
        format.Contains("png", StringComparison.OrdinalIgnoreCase)
        || format.Contains("jpeg", StringComparison.OrdinalIgnoreCase)
        || format.Contains("jpg", StringComparison.OrdinalIgnoreCase)
        || format.Contains("webp", StringComparison.OrdinalIgnoreCase)
        || format.Contains("bitmap", StringComparison.OrdinalIgnoreCase)
        || format.Contains("dib", StringComparison.OrdinalIgnoreCase)
        || format.Contains("image", StringComparison.OrdinalIgnoreCase);

    private static bool IsTextLikeFormat(string format) =>
        format.Equals(DataFormats.Text, StringComparison.OrdinalIgnoreCase)
        || format.Equals("Text", StringComparison.OrdinalIgnoreCase)
        || format.Equals("UnicodeText", StringComparison.OrdinalIgnoreCase)
        || format.Contains("html", StringComparison.OrdinalIgnoreCase)
        || format.Contains("csv", StringComparison.OrdinalIgnoreCase)
        || format.Contains("rtf", StringComparison.OrdinalIgnoreCase)
        || format.Contains("xml", StringComparison.OrdinalIgnoreCase);

    private static bool IsPng(byte[] bytes) =>
        bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

    private static bool IsJpeg(byte[] bytes) =>
        bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

    private static bool IsWebp(byte[] bytes) =>
        bytes.Length > 12
        && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
        && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P';

    private static string ContentTypeOf(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".webp" => "image/webp",
        _ => "image/png"
    };

    private static (string Name, string Type) NameAndType(string format, byte[] bytes)
    {
        if (IsJpeg(bytes) || format.Contains("jpeg", StringComparison.OrdinalIgnoreCase) || format.Contains("jpg", StringComparison.OrdinalIgnoreCase))
        {
            return ($"paste-{DateTime.Now:yyyyMMddHHmmss}.jpg", "image/jpeg");
        }

        if (IsWebp(bytes) || format.Contains("webp", StringComparison.OrdinalIgnoreCase))
        {
            return ($"paste-{DateTime.Now:yyyyMMddHHmmss}.webp", "image/webp");
        }

        return ($"paste-{DateTime.Now:yyyyMMddHHmmss}.png", "image/png");
    }
}
