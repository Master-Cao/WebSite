using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Application.Mapping;
using YJCabin.Domain.Common;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Services;

public sealed class ArticleService : IArticleService
{
    private readonly IArticleRepository _articles;
    private readonly ITagRepository _tags;
    private readonly IArticleContentStore _contentStore;
    private readonly IMarkdownRenderer _markdown;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleService(
        IArticleRepository articles,
        ITagRepository tags,
        IArticleContentStore contentStore,
        IMarkdownRenderer markdown,
        IUnitOfWork unitOfWork)
    {
        _articles = articles;
        _tags = tags;
        _contentStore = contentStore;
        _markdown = markdown;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ArticleSummaryDto>> ListAsync(ListQuery query, bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _articles.ListAsync(query, publishedOnly, cancellationToken);
        return new PagedResult<ArticleSummaryDto>
        {
            Items = items.Select(x => x.ToSummary()).ToList(),
            Page = query.SafePage,
            PageSize = query.SafePageSize,
            TotalCount = total
        };
    }

    public async Task<ArticleDetailDto> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetBySlugAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Article", slug);
        if (publishedOnly && article.Status != ContentStatus.Published)
        {
            throw new NotFoundException("Article", slug);
        }

        var document = await _contentStore.ReadAsync(article.Slug, cancellationToken)
                       ?? throw new NotFoundException("Article file", slug);
        return ToDetail(article, document);
    }

    public async Task<ArticleDetailDto> CreateAsync(UpsertArticleRequest request, CancellationToken cancellationToken = default)
    {
        var slug = await AllocateSlugAsync(request.Title, null, cancellationToken);

        var article = new Article { Slug = slug };
        var document = await PersistAsync(article, request, slug, cancellationToken);
        await _articles.AddAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDetail(article, document);
    }

    public async Task<ArticleDetailDto> UpdateAsync(string slug, UpsertArticleRequest request, CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetBySlugAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Article", slug);
        var document = await PersistAsync(article, request, article.Slug, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDetail(article, document);
    }

    public async Task DeleteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetBySlugAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Article", slug);
        await _contentStore.DeleteAsync(article.Slug, cancellationToken);
        _articles.Remove(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ArticleDocument> PersistAsync(
        Article article,
        UpsertArticleRequest request,
        string slug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Markdown))
        {
            throw new ValidationException("title", "标题和正文不能为空。");
        }

        var publishedAt = DtoMapper.ResolvePublishedAt(request.Status, request.PublishedAt);
        var document = new ArticleDocument
        {
            Slug = slug,
            Title = request.Title.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            CoverUrl = request.CoverUrl,
            PublishedAt = publishedAt,
            Draft = request.Status != ContentStatus.Published,
            Tags = request.Tags,
            MarkdownBody = request.Markdown,
            RawFile = string.Empty,
            Hash = string.Empty,
            RelativePath = $"articles/{slug}.md"
        };

        document = await _contentStore.WriteAsync(document, cancellationToken);

        var tags = await _tags.GetOrCreateManyAsync(request.Tags, cancellationToken);
        article.Slug = slug;
        article.Title = document.Title;
        article.Summary = document.Summary;
        article.CoverUrl = document.CoverUrl;
        article.Status = request.Status;
        article.PublishedAt = publishedAt;
        article.FilePath = document.RelativePath;
        article.ContentHash = document.Hash;
        article.UpdatedAt = DateTimeOffset.UtcNow;
        article.ArticleTags.Clear();
        foreach (var tag in tags)
        {
            article.ArticleTags.Add(new ArticleTag { Article = article, Tag = tag });
        }

        return document;
    }

    private async Task<string> AllocateSlugAsync(string title, Guid? exceptId, CancellationToken cancellationToken)
    {
        var root = SlugHelper.FromTitle(title);
        var slug = root;
        var suffix = 2;
        while (await _articles.SlugExistsAsync(slug, exceptId, cancellationToken))
        {
            slug = $"{root}-{suffix++}";
        }

        return slug;
    }

    private ArticleDetailDto ToDetail(Article article, ArticleDocument document) => new()
    {
        Id = article.Id,
        Slug = article.Slug,
        Title = article.Title,
        Summary = article.Summary,
        CoverUrl = article.CoverUrl,
        Status = article.Status,
        PublishedAt = article.PublishedAt,
        Tags = article.ArticleTags.ToTagDtos(),
        Markdown = document.MarkdownBody,
        Html = _markdown.ToHtml(document.MarkdownBody)
    };
}
