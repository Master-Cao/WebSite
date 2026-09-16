using YJCabin.Domain.Common;

namespace YJCabin.Application.Dtos;

public class ArticleSummaryDto
{
    public Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public string? CoverUrl { get; init; }
    public ContentStatus Status { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
}

public sealed class ArticleDetailDto : ArticleSummaryDto
{
    public required string Markdown { get; init; }
    public required string Html { get; init; }
}

public sealed class UpsertArticleRequest
{
    public string? Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public string? CoverUrl { get; init; }
    public ContentStatus Status { get; init; } = ContentStatus.Draft;
    public DateTimeOffset? PublishedAt { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public required string Markdown { get; init; }
}
