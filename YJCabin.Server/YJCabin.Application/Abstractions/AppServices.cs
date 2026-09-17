using YJCabin.Application.Common;
using YJCabin.Application.Dtos;

namespace YJCabin.Application.Abstractions;

public interface IProjectService
{
    Task<PagedResult<ProjectSummaryDto>> ListAsync(ListQuery query, bool publishedOnly, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> CreateAsync(UpsertProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> UpdateAsync(string slug, UpsertProjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string slug, CancellationToken cancellationToken = default);
}

public interface IArticleService
{
    Task<PagedResult<ArticleSummaryDto>> ListAsync(ListQuery query, bool publishedOnly, CancellationToken cancellationToken = default);
    Task<ArticleDetailDto> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken cancellationToken = default);
    Task<ArticleDetailDto> CreateAsync(UpsertArticleRequest request, CancellationToken cancellationToken = default);
    Task<ArticleDetailDto> UpdateAsync(string slug, UpsertArticleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string slug, CancellationToken cancellationToken = default);
}

public interface IAboutService
{
    Task<AboutDto> GetAsync(CancellationToken cancellationToken = default);
    Task<AboutDto> UpdateAsync(UpdateAboutRequest request, CancellationToken cancellationToken = default);
}

public interface IContactService
{
    Task SubmitAsync(CreateContactRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ContactMessageDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ContactMessageDto> PatchAsync(Guid id, PatchContactMessageRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}

public interface IMediaService
{
    Task<MediaAssetDto> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<TagDto> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
