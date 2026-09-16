using YJCabin.Application.Abstractions;
using YJCabin.Application.Dtos;
using YJCabin.Application.Mapping;

namespace YJCabin.Application.Services;

public sealed class TagService : ITagService
{
    private readonly ITagRepository _tags;

    public TagService(ITagRepository tags)
    {
        _tags = tags;
    }

    public async Task<IReadOnlyList<TagDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tags.ListAsync(cancellationToken);
        return tags.Select(x => x.ToDto()).ToList();
    }
}
