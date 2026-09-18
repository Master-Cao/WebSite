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

    private readonly HtmlSanitizer _sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedClasses.Clear();
        return sanitizer;
    }

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var html = Markdig.Markdown.ToHtml(normalized, Pipeline);
        html = html.Replace("src=\"/uploads/", "src=\"/api/uploads/", StringComparison.Ordinal);
        return _sanitizer.Sanitize(html);
    }
}
