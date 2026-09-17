using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;

namespace YJCabin.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        _auth.LoginAsync(request, cancellationToken);

    [HttpPost("refresh")]
    public Task<AuthResponse> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken) =>
        _auth.RefreshAsync(request, cancellationToken);

    [Authorize]
    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _auth.ChangePasswordAsync(GetUserId(), request, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var id))
        {
            throw new UnauthorizedAppException();
        }

        return id;
    }
}
