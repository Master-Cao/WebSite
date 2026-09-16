using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class Tag : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
}
