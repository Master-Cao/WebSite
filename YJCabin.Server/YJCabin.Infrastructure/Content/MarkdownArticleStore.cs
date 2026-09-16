using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Options;

namespace YJCabin.Infrastructure.Content;

public sealed class MarkdownArticleStore : IArticleContentStore
{
    private readonly ContentOptions _options;
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public MarkdownArticleStore(IOptions<ContentOptions> options)
    {
        _options = options.Value;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
    }

    public async Task<ArticleDocument?> ReadAsync(string slug, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(slug);
        if (!File.Exists(path))
        {
            return null;
        }

        var raw = await File.ReadAllTextAsync(path, cancellationToken);
        return Parse(slug, raw);
    }

    public async Task<ArticleDocument> WriteAsync(ArticleDocument document, CancellationToken cancellationToken = default)
    {
        var raw = BuildRawFile(document);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        var relative = $"articles/{document.Slug}.md";
        var path = Path.Combine(GetArticlesRoot(), $"{document.Slug}.md");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, raw, cancellationToken);
        return document with { RawFile = raw, Hash = hash, RelativePath = relative };
    }

    public Task DeleteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(slug);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public string BuildRawFile(ArticleDocument document)
    {
        var frontMatter = _serializer.Serialize(new ArticleFrontMatter
        {
            Title = document.Title,
            Summary = document.Summary,
            Tags = document.Tags.ToList(),
            Cover = document.CoverUrl,
            PublishedAt = document.PublishedAt?.UtcDateTime,
            Draft = document.Draft
        }).TrimEnd();

        return $"---\n{frontMatter}\n---\n\n{document.MarkdownBody.Trim()}\n";
    }

    public ArticleDocument Parse(string slug, string raw)
    {
        var (frontMatter, body) = SplitFrontMatter(raw);
        var meta = string.IsNullOrWhiteSpace(frontMatter)
            ? new ArticleFrontMatter()
            : _deserializer.Deserialize<ArticleFrontMatter>(frontMatter) ?? new ArticleFrontMatter();

        DateTimeOffset? published = meta.PublishedAt is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(meta.PublishedAt.Value, DateTimeKind.Utc));

        return new ArticleDocument
        {
            Slug = slug,
            Title = meta.Title ?? slug,
            Summary = meta.Summary ?? string.Empty,
            CoverUrl = meta.Cover,
            PublishedAt = published,
            Draft = meta.Draft,
            Tags = meta.Tags ?? [],
            MarkdownBody = body.Trim(),
            RawFile = raw,
            Hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))),
            RelativePath = $"articles/{slug}.md"
        };
    }

    private string GetArticlesRoot()
    {
        var root = Path.GetFullPath(_options.RootPath);
        var articles = Path.Combine(root, "articles");
        Directory.CreateDirectory(articles);
        return articles;
    }

    private string ResolvePath(string slug) => Path.Combine(GetArticlesRoot(), $"{slug}.md");

    public static (string FrontMatter, string Body) SplitFrontMatter(string raw)
    {
        if (!raw.StartsWith("---", StringComparison.Ordinal))
        {
            return (string.Empty, raw);
        }

        var end = raw.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0)
        {
            return (string.Empty, raw);
        }

        var yaml = raw[3..end].Trim('\r', '\n');
        var bodyStart = end + 4;
        var body = bodyStart < raw.Length ? raw[bodyStart..].TrimStart('\r', '\n') : string.Empty;
        return (yaml, body);
    }

    private sealed class ArticleFrontMatter
    {
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public List<string>? Tags { get; set; }
        public string? Cover { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool Draft { get; set; }
    }
}
