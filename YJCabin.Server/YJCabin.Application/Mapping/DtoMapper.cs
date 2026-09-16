using YJCabin.Application.Dtos;
using YJCabin.Domain.Common;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Mapping;

public static class DtoMapper
{
    public static TagDto ToDto(this Tag tag) => new()
    {
        Id = tag.Id,
        Name = tag.Name,
        Slug = tag.Slug
    };

    public static ProjectImageDto ToDto(this ProjectImage image) => new()
    {
        Id = image.Id,
        Url = image.Url,
        Caption = image.Caption,
        SortOrder = image.SortOrder
    };

    public static IReadOnlyList<TagDto> ToTagDtos(this IEnumerable<ProjectTag> links) =>
        links.Select(x => x.Tag.ToDto()).OrderBy(x => x.Name).ToList();

    public static IReadOnlyList<TagDto> ToTagDtos(this IEnumerable<ArticleTag> links) =>
        links.Select(x => x.Tag.ToDto()).OrderBy(x => x.Name).ToList();

    public static ProjectSummaryDto ToSummary(this Project project) => new()
    {
        Id = project.Id,
        Slug = project.Slug,
        Title = project.Title,
        Summary = project.Summary,
        CoverUrl = project.CoverUrl,
        Status = project.Status,
        SortOrder = project.SortOrder,
        PublishedAt = project.PublishedAt,
        Tags = project.ProjectTags.ToTagDtos()
    };

    public static ProjectDetailDto ToDetail(this Project project) => new()
    {
        Id = project.Id,
        Slug = project.Slug,
        Title = project.Title,
        Summary = project.Summary,
        Description = project.Description,
        CoverUrl = project.CoverUrl,
        RepoUrl = project.RepoUrl,
        LiveUrl = project.LiveUrl,
        Status = project.Status,
        SortOrder = project.SortOrder,
        PublishedAt = project.PublishedAt,
        Tags = project.ProjectTags.ToTagDtos(),
        Images = project.Images.OrderBy(x => x.SortOrder).Select(x => x.ToDto()).ToList()
    };

    public static ArticleSummaryDto ToSummary(this Article article) => new()
    {
        Id = article.Id,
        Slug = article.Slug,
        Title = article.Title,
        Summary = article.Summary,
        CoverUrl = article.CoverUrl,
        Status = article.Status,
        PublishedAt = article.PublishedAt,
        Tags = article.ArticleTags.ToTagDtos()
    };

    public static ContactMessageDto ToDto(this ContactMessage message) => new()
    {
        Id = message.Id,
        Name = message.Name,
        Email = message.Email,
        Subject = message.Subject,
        Body = message.Body,
        IsRead = message.IsRead,
        IsReplied = message.IsReplied,
        CreatedAt = message.CreatedAt,
        ReadAt = message.ReadAt
    };

    public static DateTimeOffset? ResolvePublishedAt(ContentStatus status, DateTimeOffset? requested)
    {
        if (status != ContentStatus.Published)
        {
            return requested;
        }

        return requested ?? DateTimeOffset.UtcNow;
    }
}
