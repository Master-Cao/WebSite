using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    public Task<PagedResult<Application.Dtos.ProjectSummaryDto>> List([FromQuery] ListQuery query, CancellationToken cancellationToken) =>
        _projects.ListAsync(query, publishedOnly: true, cancellationToken);

    [HttpGet("{slug}")]
    public Task<Application.Dtos.ProjectDetailDto> Get(string slug, CancellationToken cancellationToken) =>
        _projects.GetBySlugAsync(slug, publishedOnly: true, cancellationToken);
}
