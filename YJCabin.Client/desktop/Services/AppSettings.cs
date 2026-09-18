using System.Text.Json;

namespace YJCabin.Desktop.Services;

public static class AppSettings
{
    public const string DefaultApiBaseUrl = "https://yjcabin.top";

    public static string ResolveApiBaseUrl()
    {
        var fromEnv = Environment.GetEnvironmentVariable("YJCABIN_API_URL");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return Normalize(fromEnv);
        }

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("ApiBaseUrl", out var url)
                    && url.GetString() is { Length: > 0 } value)
                {
                    return Normalize(value);
                }
            }
        }
        catch
        {
            // Fall back to the published site.
        }

        return DefaultApiBaseUrl;
    }

    private static string Normalize(string value) => value.Trim().TrimEnd('/');
}
