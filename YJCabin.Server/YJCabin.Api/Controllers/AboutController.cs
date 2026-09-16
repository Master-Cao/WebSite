using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/about")]
public sealed class AboutController : ControllerBase
{
    private readonly IAboutService _about;

    public AboutController(IAboutService about)
    {
        _about = about;
    }

    [HttpGet]
    public Task<AboutDto> Get(CancellationToken cancellationToken) => _about.GetAsync(cancellationToken);
}
