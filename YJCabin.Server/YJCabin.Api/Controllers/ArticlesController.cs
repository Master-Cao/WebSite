using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController : ControllerBase
{
    private readonly IArticleService _articles;

    public ArticlesController(IArticleService articles)
    {
        _articles = articles;
    }

    [HttpGet]
    public Task<PagedResult<Application.Dtos.ArticleSummaryDto>> List([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
        _articles.ListAsync(query, publishedOnly: true, cancellationToken);

    [HttpGet("{slug}")]
    public Task<Application.Dtos.ArticleDetailDto> Get(string slug, CancellationToken cancellationToken) =>
        _articles.GetBySlugAsync(slug, publishedOnly: true, cancellationToken);
}
