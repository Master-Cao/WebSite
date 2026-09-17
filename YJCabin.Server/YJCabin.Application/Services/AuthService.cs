using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Application.Options;
using Microsoft.Extensions.Options;

namespace YJCabin.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAppException();
        }

        return await IssueAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAppException("Refresh token is invalid or expired.");
        }

        return await IssueAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var current = request.CurrentPassword?.Trim() ?? "";
        var next = request.NewPassword?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next))
        {
            throw new ValidationException("password", "当前密码和新密码都不能为空。");
        }

        if (next.Length < 8)
        {
            throw new ValidationException("newPassword", "新密码至少 8 位。");
        }

        if (current == next)
        {
            throw new ValidationException("newPassword", "新密码不能与当前密码相同。");
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedAppException();
        if (!_passwordHasher.Verify(user.PasswordHash, current))
        {
            throw new ValidationException("currentPassword", "当前密码不正确。");
        }

        user.PasswordHash = _passwordHasher.Hash(next);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueAsync(Domain.Entities.User user, CancellationToken cancellationToken)
    {
        var (token, expiresAt) = _jwt.CreateAccessToken(user.Id, user.UserName, user.Role);
        user.RefreshToken = _jwt.CreateRefreshToken();
        user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthResponse
        {
            AccessToken = token,
            RefreshToken = user.RefreshToken,
            ExpiresAt = expiresAt,
            UserName = user.UserName,
            Role = user.Role
        };
    }
}
