using Microsoft.Extensions.Options;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Options;

namespace YJCabin.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".md"
    };

    private readonly ContentOptions _options;

    public LocalFileStorage(IOptions<ContentOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StoredFile> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".md"
            };
        }

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var folder = Path.GetFullPath(_options.UploadsPath);
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, storedName);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);
        var size = file.Length;

        return new StoredFile
        {
            FileName = storedName,
            Url = $"{_options.PublicUploadsBase.TrimEnd('/')}/{storedName}",
            ContentType = contentType,
            Size = size
        };
    }
}
