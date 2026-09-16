using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class ProjectImage : Entity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}
