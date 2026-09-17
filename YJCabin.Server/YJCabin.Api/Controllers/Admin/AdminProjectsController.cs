using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/projects")]
public sealed class AdminProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public AdminProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    public Task<PagedResult<ProjectSummaryDto>> List([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
        _projects.ListAsync(query, publishedOnly: false, cancellationToken);

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProjectDetailDto>> Get(string slug, CancellationToken cancellationToken)
    {
        if (!SlugHelper.IsUsable(slug))
        {
            return NotFound();
        }

        return await _projects.GetBySlugAsync(slug, publishedOnly: false, cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDetailDto>> Create(UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        var created = await _projects.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { slug = created.Slug }, created);
    }

    [HttpPut("{slug}")]
    public Task<ProjectDetailDto> Update(string slug, UpsertProjectRequest request, CancellationToken cancellationToken) =>
        _projects.UpdateAsync(slug, request, cancellationToken);

    [HttpDelete("{slug}")]
    public async Task<IActionResult> Delete(string slug, CancellationToken cancellationToken)
    {
        await _projects.DeleteAsync(slug, cancellationToken);
        return NoContent();
    }
}
