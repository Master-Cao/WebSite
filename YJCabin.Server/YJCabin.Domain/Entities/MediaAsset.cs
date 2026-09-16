using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class MediaAsset : Entity
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}
