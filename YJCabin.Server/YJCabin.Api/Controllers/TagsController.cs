using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/tags")]
public sealed class TagsController : ControllerBase
{
    private readonly ITagService _tags;

    public TagsController(ITagService tags)
    {
        _tags = tags;
    }

    [HttpGet]
    public Task<IReadOnlyList<TagDto>> List(CancellationToken cancellationToken) => _tags.ListAsync(cancellationToken);
}
