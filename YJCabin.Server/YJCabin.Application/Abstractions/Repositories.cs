using YJCabin.Application.Common;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IProjectRepository
{
    Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> ListAsync(
        ListQuery query,
        bool publishedOnly,
        CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Remove(Project project);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
}

public interface IArticleRepository
{
    Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Article> Items, int TotalCount)> ListAsync(
        ListQuery query,
        bool publishedOnly,
        CancellationToken cancellationToken = default);
    Task AddAsync(Article article, CancellationToken cancellationToken = default);
    void Remove(Article article);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
}

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetOrCreateManyAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}

public interface IAboutRepository
{
    Task<AboutPage?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(AboutPage about, CancellationToken cancellationToken = default);
}

public interface IContactRepository
{
    Task AddAsync(ContactMessage message, CancellationToken cancellationToken = default);
    Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ContactMessage> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}

public interface IMediaRepository
{
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default);
}
