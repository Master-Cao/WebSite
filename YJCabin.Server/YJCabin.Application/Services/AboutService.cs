using System.Text.Json;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Services;

public sealed class AboutService : IAboutService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IAboutRepository _abouts;
    private readonly IMarkdownRenderer _markdown;
    private readonly IUnitOfWork _unitOfWork;

    public AboutService(IAboutRepository abouts, IMarkdownRenderer markdown, IUnitOfWork unitOfWork)
    {
        _abouts = abouts;
        _markdown = markdown;
        _unitOfWork = unitOfWork;
    }

    public async Task<AboutDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var about = await _abouts.GetAsync(cancellationToken)
                    ?? throw new NotFoundException("About", "current");
        return ToDto(about);
    }

    public async Task<AboutDto> UpdateAsync(UpdateAboutRequest request, CancellationToken cancellationToken = default)
    {
        var about = await _abouts.GetAsync(cancellationToken);
        if (about is null)
        {
            about = new AboutPage();
            await _abouts.AddAsync(about, cancellationToken);
        }

        about.Headline = request.Headline.Trim();
        about.BioMarkdown = request.BioMarkdown;
        about.SkillsJson = JsonSerializer.Serialize(request.Skills, JsonOptions);
        about.SocialLinksJson = JsonSerializer.Serialize(request.SocialLinks, JsonOptions);
        about.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(about);
    }

    private AboutDto ToDto(AboutPage about)
    {
        var skills = JsonSerializer.Deserialize<List<string>>(about.SkillsJson, JsonOptions) ?? [];
        var social = JsonSerializer.Deserialize<List<SocialLinkDto>>(about.SocialLinksJson, JsonOptions) ?? [];
        return new AboutDto
        {
            Id = about.Id,
            Headline = about.Headline,
            BioMarkdown = about.BioMarkdown,
            BioHtml = _markdown.ToHtml(about.BioMarkdown),
            Skills = skills,
            SocialLinks = social
        };
    }
}
