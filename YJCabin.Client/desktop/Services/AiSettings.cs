using System.Text.Json;

namespace YJCabin.Desktop.Services;

public sealed class AiSettings
{
    public const string DefaultBaseUrl = "https://api.moonshot.cn/v1";
    public const string DefaultModel = "kimi-k3";

    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public string Model { get; set; } = DefaultModel;
    public string ApiKey { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(Model) &&
        !string.IsNullOrWhiteSpace(ApiKey);

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YJCabin",
        "ai-settings.json");

    public static AiSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<AiSettings>(File.ReadAllText(FilePath));
                if (loaded is not null)
                {
                    if (LooksLikePreviousDefault(loaded))
                    {
                        loaded.BaseUrl = DefaultBaseUrl;
                        loaded.Model = DefaultModel;
                    }

                    return loaded;
                }
            }
        }
        catch
        {
            // Keep defaults if the local file is missing or corrupt.
        }

        return new AiSettings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    private static bool LooksLikePreviousDefault(AiSettings loaded) =>
        loaded.BaseUrl.Contains("deepseek.com", StringComparison.OrdinalIgnoreCase) ||
        loaded.Model.Contains("deepseek", StringComparison.OrdinalIgnoreCase);
}
