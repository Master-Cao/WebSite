using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace YJCabin.Desktop.Services;

internal static class AiSummaryClient
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };
    private static readonly Regex ImageMarkdown = new(@"!\[[^\]]*\]\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex ExtraBlank = new(@"\n{3,}", RegexOptions.Compiled);

    public static async Task<string> SummarizeArticleAsync(string title, string markdown, CancellationToken cancellationToken = default)
    {
        var settings = AiSettings.Load();
        if (!settings.IsConfigured)
        {
            throw new InvalidOperationException("请先在设置里填写 AI 接口地址、模型和 API Key。");
        }

        var body = PrepareBody(title, markdown);
        var endpoint = ChatCompletionsUrl(settings.BaseUrl);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = settings.Model.Trim(),
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "你是 Kimi，由 Moonshot AI 提供的人工智能助手。现在请担任中文博客编辑：根据文章标题和正文写一段列表/卡片用的摘要。只输出摘要正文，不要标题、引号、前缀或解释。使用简体中文，80到120字，信息具体，不要空泛套话。"
                    },
                    new { role = "user", content = body }
                }
            }), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey.Trim());

        using var response = await Http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadError(json, response.ReasonPhrase));
        }

        using var doc = JsonDocument.Parse(json);
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");
        var text = message.TryGetProperty("content", out var content) ? content.GetString() : null;
        if (string.IsNullOrWhiteSpace(text) &&
            message.TryGetProperty("reasoning_content", out var reasoning))
        {
            text = reasoning.GetString();
        }
        text = CleanSummary(text);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("AI 没有返回可用摘要，请稍后重试。");
        }

        return text;
    }

    private static string PrepareBody(string title, string markdown)
    {
        var text = ImageMarkdown.Replace(markdown ?? "", "[图片]");
        text = ExtraBlank.Replace(text.Replace("\r\n", "\n", StringComparison.Ordinal), "\n\n").Trim();
        if (text.Length > 8000)
        {
            text = text[..8000];
        }

        var heading = string.IsNullOrWhiteSpace(title) ? "无标题" : title.Trim();
        return $"标题：{heading}\n\n正文：\n{text}";
    }

    private static string ChatCompletionsUrl(string baseUrl)
    {
        var root = baseUrl.Trim().TrimEnd('/');
        if (root.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return root;
        }

        return root + "/chat/completions";
    }

    private static string CleanSummary(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        var value = text.Trim().Trim('"', '“', '”', '\'');
        value = value.Replace("摘要：", "", StringComparison.Ordinal).Replace("摘要:", "", StringComparison.Ordinal).Trim();
        return value;
    }

    private static string ReadError(string json, string? fallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var message) &&
                    message.GetString() is { Length: > 0 } text)
                {
                    return text;
                }

                if (error.ValueKind == JsonValueKind.String && error.GetString() is { Length: > 0 } simple)
                {
                    return simple;
                }
            }
        }
        catch
        {
            // Use the HTTP reason if the body is not JSON.
        }

        return string.IsNullOrWhiteSpace(json) ? fallback ?? "AI 请求失败。" : json;
    }
}
