namespace YJCabin.Application.Abstractions;

public sealed record ArticleDocument
{
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public string? CoverUrl { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public bool Draft { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public required string MarkdownBody { get; init; }
    public required string RawFile { get; init; }
    public required string Hash { get; init; }
    public required string RelativePath { get; init; }
}

public interface IArticleContentStore
{
    Task<ArticleDocument?> ReadAsync(string slug, CancellationToken cancellationToken = default);
    Task<ArticleDocument> WriteAsync(ArticleDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(string slug, CancellationToken cancellationToken = default);
    string BuildRawFile(ArticleDocument document);
}

public interface IMarkdownRenderer
{
    string ToHtml(string markdown);
}

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public sealed class StoredFile
{
    public required string FileName { get; init; }
    public required string Url { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(Guid userId, string userName, string role);
    string CreateRefreshToken();
}
