using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Application.Mapping;

namespace YJCabin.Application.Services;

public sealed class TagService : ITagService
{
    private readonly ITagRepository _tags;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(ITagRepository tags, IUnitOfWork unitOfWork)
    {
        _tags = tags;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TagDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tags.ListAsync(cancellationToken);
        return tags.Select(x => x.ToDto()).ToList();
    }

    public async Task<TagDto> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ValidationException("name", "标签名称不能为空。");
        }

        if (await _tags.FindByNameAsync(trimmed, cancellationToken) is not null)
        {
            throw new ConflictException($"标签「{trimmed}」已存在。");
        }

        var created = await _tags.GetOrCreateManyAsync([trimmed], cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return created[0].ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tags.GetByIdAsync(id, cancellationToken)
                  ?? throw new NotFoundException("Tag", id.ToString());
        tag.ArticleTags.Clear();
        tag.ProjectTags.Clear();
        _tags.Remove(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
