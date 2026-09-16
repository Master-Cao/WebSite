namespace YJCabin.Application.Dtos;

public sealed class SocialLinkDto
{
    public required string Name { get; init; }
    public required string Url { get; init; }
}

public sealed class AboutDto
{
    public Guid Id { get; init; }
    public required string Headline { get; init; }
    public required string BioMarkdown { get; init; }
    public required string BioHtml { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<SocialLinkDto> SocialLinks { get; init; } = [];
}

public sealed class UpdateAboutRequest
{
    public required string Headline { get; init; }
    public required string BioMarkdown { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<SocialLinkDto> SocialLinks { get; init; } = [];
}
