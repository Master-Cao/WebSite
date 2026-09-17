using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/about")]
public sealed class AdminAboutController : ControllerBase
{
    private readonly IAboutService _about;

    public AdminAboutController(IAboutService about)
    {
        _about = about;
    }

    [HttpGet]
    public Task<AboutDto> Get(CancellationToken cancellationToken) => _about.GetAsync(cancellationToken);

    [HttpPut]
    public Task<AboutDto> Update(UpdateAboutRequest request, CancellationToken cancellationToken) =>
        _about.UpdateAsync(request, cancellationToken);
}

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/contact-messages")]
public sealed class AdminContactController : ControllerBase
{
    private readonly IContactService _contact;

    public AdminContactController(IContactService contact)
    {
        _contact = contact;
    }

    [HttpGet]
    public Task<PagedResult<ContactMessageDto>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default) =>
        _contact.ListAsync(page, pageSize, cancellationToken);

    [HttpPatch("{id:guid}")]
    public Task<ContactMessageDto> Patch(Guid id, PatchContactMessageRequest request, CancellationToken cancellationToken) =>
        _contact.PatchAsync(id, request, cancellationToken);
}

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/media")]
public sealed class AdminMediaController : ControllerBase
{
    private readonly IMediaService _media;

    public AdminMediaController(IMediaService media)
    {
        _media = media;
    }

    [HttpPost]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<ActionResult<MediaAssetDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiError { Code = "validation_error", Message = "File is required." });
        }

        await using var stream = file.OpenReadStream();
        var asset = await _media.UploadAsync(stream, file.FileName, file.ContentType, cancellationToken);
        return Created(asset.Url, asset);
    }
}

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/tags")]
public sealed class AdminTagsController : ControllerBase
{
    private readonly ITagService _tags;

    public AdminTagsController(ITagService tags)
    {
        _tags = tags;
    }

    [HttpGet]
    public Task<IReadOnlyList<TagDto>> List(CancellationToken cancellationToken) =>
        _tags.ListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<TagDto>> Create(CreateTagRequest request, CancellationToken cancellationToken)
    {
        var created = await _tags.CreateAsync(request.Name, cancellationToken);
        return CreatedAtAction(nameof(List), created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _tags.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
