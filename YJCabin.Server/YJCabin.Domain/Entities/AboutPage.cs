using YJCabin.Domain.Common;

namespace YJCabin.Domain.Entities;

public class AboutPage : Entity
{
    public string Headline { get; set; } = string.Empty;
    public string BioMarkdown { get; set; } = string.Empty;
    public string SkillsJson { get; set; } = "[]";
    public string SocialLinksJson { get; set; } = "[]";
}
