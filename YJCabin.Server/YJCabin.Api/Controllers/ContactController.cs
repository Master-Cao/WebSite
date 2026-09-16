using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/contact")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactService _contact;

    public ContactController(IContactService contact)
    {
        _contact = contact;
    }

    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit(CreateContactRequest request, CancellationToken cancellationToken)
    {
        await _contact.SubmitAsync(request, cancellationToken);
        return Accepted();
    }
}
