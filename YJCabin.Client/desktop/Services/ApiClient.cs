using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YJCabin.Desktop.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public string? AccessToken { get; private set; }

    public async Task LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        var auth = await PostAsync<AuthResponse>("/api/auth/login", new { userName, password }, false, cancellationToken);
        AccessToken = auth.AccessToken;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }

    public void Logout()
    {
        AccessToken = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public Task<PagedResult<ProjectSummary>> ListProjectsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<ProjectSummary>>("/api/admin/projects", true, cancellationToken);

    public Task<ProjectDetail> GetProjectAsync(string slug, CancellationToken cancellationToken = default) =>
        GetAsync<ProjectDetail>($"/api/admin/projects/{slug}", true, cancellationToken);

    public Task<ProjectDetail> SaveProjectAsync(string? slug, UpsertProjectRequest request, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(slug)
            ? PostAsync<ProjectDetail>("/api/admin/projects", request, true, cancellationToken)
            : PutAsync<ProjectDetail>($"/api/admin/projects/{slug}", request, true, cancellationToken);

    public Task DeleteProjectAsync(string slug, CancellationToken cancellationToken = default) =>
        SendAsync($"/api/admin/projects/{slug}", HttpMethod.Delete, null, true, cancellationToken);

    public Task<List<TagDto>> ListTagsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<TagDto>>("/api/tags", false, cancellationToken);

    public Task<TagDto> CreateTagAsync(string name, CancellationToken cancellationToken = default) =>
        PostAsync<TagDto>("/api/admin/tags", new { name }, true, cancellationToken);

    public Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync($"/api/admin/tags/{id}", HttpMethod.Delete, null, true, cancellationToken);

    public Task<PagedResult<ArticleSummary>> ListArticlesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<ArticleSummary>>("/api/admin/articles", true, cancellationToken);

    public Task<ArticleDetail> GetArticleAsync(string slug, CancellationToken cancellationToken = default) =>
        GetAsync<ArticleDetail>($"/api/admin/articles/{slug}", true, cancellationToken);

    public Task<ArticleDetail> SaveArticleAsync(string? slug, UpsertArticleRequest request, CancellationToken cancellationToken = default) =>
        string.IsNullOrWhiteSpace(slug)
            ? PostAsync<ArticleDetail>("/api/admin/articles", request, true, cancellationToken)
            : PutAsync<ArticleDetail>($"/api/admin/articles/{slug}", request, true, cancellationToken);

    public Task DeleteArticleAsync(string slug, CancellationToken cancellationToken = default) =>
        SendAsync($"/api/admin/articles/{slug}", HttpMethod.Delete, null, true, cancellationToken);

    public Task<AboutDto> GetAboutAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AboutDto>("/api/admin/about", true, cancellationToken);

    public Task<AboutDto> SaveAboutAsync(UpdateAboutRequest request, CancellationToken cancellationToken = default) =>
        PutAsync<AboutDto>("/api/admin/about", request, true, cancellationToken);

    public Task<PagedResult<ContactMessageDto>> ListMessagesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<ContactMessageDto>>("/api/admin/contact-messages", true, cancellationToken);

    public Task PatchMessageAsync(Guid id, bool? isRead, bool? isReplied, CancellationToken cancellationToken = default) =>
        SendAsync($"/api/admin/contact-messages/{id}", HttpMethod.Patch, new { isRead, isReplied }, true, cancellationToken);

    public Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default) =>
        SendAsync("/api/auth/password", HttpMethod.Post, new { currentPassword, newPassword }, true, cancellationToken);

    private async Task<T> GetAsync<T>(string path, bool auth, CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(path, HttpMethod.Get, null, auth, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken)
               ?? throw new InvalidOperationException("Empty response.");
    }

    private Task<T> PostAsync<T>(string path, object body, bool auth, CancellationToken cancellationToken) =>
        SendJsonAsync<T>(path, HttpMethod.Post, body, auth, cancellationToken);

    private Task<T> PutAsync<T>(string path, object body, bool auth, CancellationToken cancellationToken) =>
        SendJsonAsync<T>(path, HttpMethod.Put, body, auth, cancellationToken);

    private async Task<T> SendJsonAsync<T>(string path, HttpMethod method, object body, bool auth, CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(path, method, body, auth, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(Json, cancellationToken)
               ?? throw new InvalidOperationException("Empty response.");
    }

    private async Task SendAsync(string path, HttpMethod method, object? body, bool auth, CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(path, method, body, auth, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(string path, HttpMethod method, object? body, bool auth, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        if (auth && string.IsNullOrWhiteSpace(AccessToken))
        {
            throw new InvalidOperationException("Not authenticated.");
        }

        var response = await _http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(ReadError(text, response.ReasonPhrase));
        }

        return response;
    }

    private static string ReadError(string text, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("message", out var message) &&
                    message.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(message.GetString()))
                {
                    return message.GetString()!;
                }
            }
            catch (JsonException)
            {
            }
        }

        return string.IsNullOrWhiteSpace(text) ? fallback ?? "请求失败" : text;
    }
}

public sealed class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class ProjectSummary
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public List<TagDto> Tags { get; set; } = [];
}

public sealed class ProjectDetail : ProjectSummary
{
    public string Description { get; set; } = string.Empty;
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public int SortOrder { get; set; }
}

public sealed class UpsertProjectRequest
{
    public string? Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public int SortOrder { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<object> Images { get; set; } = [];
}

public class ArticleSummary
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public List<TagDto> Tags { get; set; } = [];
}

public sealed class ArticleDetail : ArticleSummary
{
    public string Markdown { get; set; } = string.Empty;
}

public sealed class UpsertArticleRequest
{
    public string? Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public List<string> Tags { get; set; } = [];
    public string Markdown { get; set; } = string.Empty;
}

public sealed class SocialLinkDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class AboutDto
{
    public Guid Id { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string BioMarkdown { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<SocialLinkDto> SocialLinks { get; set; } = [];
}

public sealed class UpdateAboutRequest
{
    public string Headline { get; set; } = string.Empty;
    public string BioMarkdown { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = [];
    public List<SocialLinkDto> SocialLinks { get; set; } = [];
}

public sealed class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
}

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
