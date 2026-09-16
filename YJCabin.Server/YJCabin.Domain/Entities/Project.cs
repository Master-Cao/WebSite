using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class Project : Entity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public int SortOrder { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
}
