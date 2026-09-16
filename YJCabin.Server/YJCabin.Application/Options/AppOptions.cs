namespace YJCabin.Application.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "YJCabin";
    public string Audience { get; set; } = "YJCabin.Clients";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail { get; set; } = "admin@yjcabin.local";
    public string AdminPassword { get; set; } = "ChangeMe!123";
}

public sealed class ContentOptions
{
    public const string SectionName = "Content";

    public string RootPath { get; set; } = "../content";
    public string UploadsPath { get; set; } = "wwwroot/uploads";
    public string PublicUploadsBase { get; set; } = "/uploads";
}
