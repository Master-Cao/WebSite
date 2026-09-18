using System.Text.RegularExpressions;

namespace YJCabin.Desktop.Services;

internal static class LocalMediaStore
{
    public const string Scheme = "yjcabin-local:";

    private static readonly Regex LocalRef = new(
        @"yjcabin-local:(?://)?([A-Za-z0-9._-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YJCabin",
        "media-cache");

    public static async Task<string> SaveAsync(Stream content, string fileName)
    {
        Directory.CreateDirectory(Root);
        var extension = NormalizeExtension(fileName);
        var id = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(Root, id);
        await using (var file = File.Create(path))
        {
            if (content.CanSeek)
            {
                content.Position = 0;
            }

            await content.CopyToAsync(file);
        }

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        var bytes = await File.ReadAllBytesAsync(path);
        MediaImageCache.Store(ToUrl(id), bytes);
        return id;
    }

    public static string ToUrl(string id) => Scheme + id;

    public static bool ContainsLocal(string? markdown) =>
        !string.IsNullOrEmpty(markdown) && LocalRef.IsMatch(markdown);

    public static IReadOnlyList<string> ListIds(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return [];
        }

        return LocalRef.Matches(markdown)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool TryOpen(string? url, out MemoryStream? stream)
    {
        stream = null;
        if (!TryResolvePath(url, out var path))
        {
            return false;
        }

        var bytes = File.ReadAllBytes(path);
        stream = new MemoryStream(bytes, writable: false);
        return true;
    }

    public static async Task<string> UploadAndRewriteAsync(
        string markdown,
        Func<Stream, string, string, CancellationToken, Task<MediaAssetDto>> upload,
        CancellationToken cancellationToken = default)
    {
        var rewritten = markdown;
        foreach (var id in ListIds(markdown))
        {
            if (!TryResolvePath(ToUrl(id), out var path))
            {
                throw new InvalidOperationException($"本地图片已丢失：{id}，请重新插入后再发布。");
            }

            await using var file = File.OpenRead(path);
            var asset = await upload(file, id, ContentTypeOf(id), cancellationToken);
            rewritten = rewritten.Replace(ToUrl(id), asset.Url, StringComparison.OrdinalIgnoreCase);
            rewritten = rewritten.Replace("yjcabin-local://" + id, asset.Url, StringComparison.OrdinalIgnoreCase);
        }

        return rewritten;
    }

    public static void Delete(string id)
    {
        if (!IsSafeId(id))
        {
            return;
        }

        var path = Path.Combine(Root, id);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void DeleteAll(IEnumerable<string> ids)
    {
        foreach (var id in ids)
        {
            Delete(id);
        }
    }

    private static bool TryResolvePath(string? url, out string path)
    {
        path = "";
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var match = LocalRef.Match(url.Trim());
        if (!match.Success)
        {
            return false;
        }

        var id = match.Groups[1].Value;
        if (!IsSafeId(id))
        {
            return false;
        }

        var candidate = Path.GetFullPath(Path.Combine(Root, id));
        var root = Path.GetFullPath(Root);
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(candidate))
        {
            return false;
        }

        path = candidate;
        return true;
    }

    private static bool IsSafeId(string id) =>
        id.Length is > 0 and < 80 && LocalRef.IsMatch(Scheme + id);

    private static string NormalizeExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".png" or ".jpg" or ".jpeg" or ".webp" ? extension : ".png";
    }

    private static string ContentTypeOf(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".webp" => "image/webp",
        _ => "image/png"
    };
}
