using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/articles")]
public sealed class AdminArticlesController : ControllerBase
{
    private readonly IArticleService _articles;

    public AdminArticlesController(IArticleService articles)
    {
        _articles = articles;
    }

    [HttpGet]
    public Task<PagedResult<ArticleSummaryDto>> List([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
        _articles.ListAsync(query, publishedOnly: false, cancellationToken);

    [HttpGet("{slug}")]
    public Task<ArticleDetailDto> Get(string slug, CancellationToken cancellationToken) =>
        _articles.GetBySlugAsync(slug, publishedOnly: false, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<ArticleDetailDto>> Create(UpsertArticleRequest request, CancellationToken cancellationToken)
    {
        var created = await _articles.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { slug = created.Slug }, created);
    }

    [HttpPut("{slug}")]
    public Task<ArticleDetailDto> Update(string slug, UpsertArticleRequest request, CancellationToken cancellationToken) =>
        _articles.UpdateAsync(slug, request, cancellationToken);

    [HttpDelete("{slug}")]
    public async Task<IActionResult> Delete(string slug, CancellationToken cancellationToken)
    {
        await _articles.DeleteAsync(slug, cancellationToken);
        return NoContent();
    }
}
