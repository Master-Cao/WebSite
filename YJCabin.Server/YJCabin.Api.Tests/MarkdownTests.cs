using YJCabin.Infrastructure.Content;
using YJCabin.Infrastructure.Markdown;

namespace YJCabin.Api.Tests;

public class MarkdownTests
{
    [Fact]
    public void SplitFrontMatter_ReadsYamlAndBody()
    {
        var raw = """
                  ---
                  title: Hello
                  draft: true
                  ---

                  Body text
                  """;

        var (yaml, body) = MarkdownArticleStore.SplitFrontMatter(raw);
        Assert.Contains("title: Hello", yaml);
        Assert.Equal("Body text", body.Trim());
    }

    [Fact]
    public void Renderer_ProducesSafeHtml()
    {
        var html = new MarkdownRenderer().ToHtml("# Title\n\n<script>alert(1)</script>\n\nHello");
        Assert.Contains("<h1>", html);
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("Hello", html);
    }
}
