namespace YJCabin.Application.Dtos;

public sealed class LoginRequest
{
    public required string UserName { get; init; }
    public required string Password { get; init; }
}

public sealed class RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public sealed class ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}

public sealed class AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public required string UserName { get; init; }
    public required string Role { get; init; }
}

public sealed class MediaAssetDto
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string Url { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
}
