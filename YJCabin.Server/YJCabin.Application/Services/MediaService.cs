using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Services;

public sealed class MediaService : IMediaService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "text/markdown", "text/plain"
    };

    private readonly IFileStorage _storage;
    private readonly IMediaRepository _media;
    private readonly IUnitOfWork _unitOfWork;

    public MediaService(IFileStorage storage, IMediaRepository media, IUnitOfWork unitOfWork)
    {
        _storage = storage;
        _media = media;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediaAssetDto> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!AllowedTypes.Contains(contentType))
        {
            throw new ValidationException("contentType", "Only jpeg, png, webp and markdown uploads are allowed.");
        }

        var stored = await _storage.SaveAsync(content, fileName, contentType, cancellationToken);
        var asset = new MediaAsset
        {
            FileName = stored.FileName,
            Url = stored.Url,
            ContentType = stored.ContentType,
            Size = stored.Size
        };
        await _media.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new MediaAssetDto
        {
            Id = asset.Id,
            FileName = asset.FileName,
            Url = asset.Url,
            ContentType = asset.ContentType,
            Size = asset.Size
        };
    }
}
