using YJCabin.Domain.Common;

namespace YJCabin.Application.Dtos;

public sealed class TagDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
}

public sealed class ProjectImageDto
{
    public Guid Id { get; init; }
    public required string Url { get; init; }
    public string? Caption { get; init; }
    public int SortOrder { get; init; }
}

public class ProjectSummaryDto
{
    public Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public string? CoverUrl { get; init; }
    public ContentStatus Status { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
}

public sealed class ProjectDetailDto : ProjectSummaryDto
{
    public required string Description { get; init; }
    public string? RepoUrl { get; init; }
    public string? LiveUrl { get; init; }
    public IReadOnlyList<ProjectImageDto> Images { get; init; } = [];
}

public sealed class ProjectImageInput
{
    public required string Url { get; init; }
    public string? Caption { get; init; }
    public int SortOrder { get; init; }
}

public sealed class UpsertProjectRequest
{
    public string? Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public string? CoverUrl { get; init; }
    public string? RepoUrl { get; init; }
    public string? LiveUrl { get; init; }
    public ContentStatus Status { get; init; } = ContentStatus.Draft;
    public int SortOrder { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<ProjectImageInput> Images { get; init; } = [];
}
