using Ganss.Xss;
using Markdig;
using YJCabin.Application.Abstractions;

namespace YJCabin.Infrastructure.Markdown;

public sealed class MarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private readonly HtmlSanitizer _sanitizer = new();

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdig.Markdown.ToHtml(markdown, Pipeline);
        return _sanitizer.Sanitize(html);
    }
}
