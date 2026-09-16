using Microsoft.EntityFrameworkCore;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Domain.Common;
using YJCabin.Domain.Entities;

namespace YJCabin.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        IncludeAll(_db.Projects).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        IncludeAll(_db.Projects).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> ListAsync(
        ListQuery query,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var source = IncludeAll(_db.Projects).AsQueryable();
        if (publishedOnly)
        {
            source = source.Where(x => x.Status == ContentStatus.Published);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim().ToLower();
            source = source.Where(x => x.ProjectTags.Any(t => t.Tag.Slug == tag || t.Tag.Name.ToLower() == tag));
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = query.Q.Trim().ToLower();
            source = source.Where(x => x.Title.ToLower().Contains(q) || x.Summary.ToLower().Contains(q));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.PublishedAt)
            .Skip((query.SafePage - 1) * query.SafePageSize)
            .Take(query.SafePageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        _db.Projects.AddAsync(project, cancellationToken).AsTask();

    public void Remove(Project project) => _db.Projects.Remove(project);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) =>
        _db.Projects.AnyAsync(x => x.Slug == slug && (exceptId == null || x.Id != exceptId), cancellationToken);

    private static IQueryable<Project> IncludeAll(IQueryable<Project> source) =>
        source.Include(x => x.Images).Include(x => x.ProjectTags).ThenInclude(x => x.Tag);
}

public sealed class ArticleRepository : IArticleRepository
{
    private readonly AppDbContext _db;

    public ArticleRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        IncludeAll(_db.Articles).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        IncludeAll(_db.Articles).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Article> Items, int TotalCount)> ListAsync(
        ListQuery query,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var source = IncludeAll(_db.Articles).AsQueryable();
        if (publishedOnly)
        {
            source = source.Where(x => x.Status == ContentStatus.Published);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim().ToLower();
            source = source.Where(x => x.ArticleTags.Any(t => t.Tag.Slug == tag || t.Tag.Name.ToLower() == tag));
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var q = query.Q.Trim().ToLower();
            source = source.Where(x => x.Title.ToLower().Contains(q) || x.Summary.ToLower().Contains(q));
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((query.SafePage - 1) * query.SafePageSize)
            .Take(query.SafePageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task AddAsync(Article article, CancellationToken cancellationToken = default) =>
        _db.Articles.AddAsync(article, cancellationToken).AsTask();

    public void Remove(Article article) => _db.Articles.Remove(article);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) =>
        _db.Articles.AnyAsync(x => x.Slug == slug && (exceptId == null || x.Id != exceptId), cancellationToken);

    private static IQueryable<Article> IncludeAll(IQueryable<Article> source) =>
        source.Include(x => x.ArticleTags).ThenInclude(x => x.Tag);
}

public sealed class TagRepository : ITagRepository
{
    private readonly AppDbContext _db;

    public TagRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Tag>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Tags.OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetOrCreateManyAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var result = new List<Tag>();
        foreach (var raw in names.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var slug = SlugHelper.From(raw);
            var existing = await _db.Tags.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
            if (existing is null)
            {
                existing = new Tag { Name = raw, Slug = slug };
                await _db.Tags.AddAsync(existing, cancellationToken);
            }

            result.Add(existing);
        }

        return result;
    }
}

public sealed class AboutRepository : IAboutRepository
{
    private readonly AppDbContext _db;

    public AboutRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<AboutPage?> GetAsync(CancellationToken cancellationToken = default) =>
        _db.AboutPages.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    public Task AddAsync(AboutPage about, CancellationToken cancellationToken = default) =>
        _db.AboutPages.AddAsync(about, cancellationToken).AsTask();
}

public sealed class ContactRepository : IContactRepository
{
    private readonly AppDbContext _db;

    public ContactRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(ContactMessage message, CancellationToken cancellationToken = default) =>
        _db.ContactMessages.AddAsync(message, cancellationToken).AsTask();

    public Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ContactMessage> Items, int TotalCount)> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var source = _db.ContactMessages.AsQueryable();
        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);

    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        _db.Users.AddAsync(user, cancellationToken).AsTask();
}

public sealed class MediaRepository : IMediaRepository
{
    private readonly AppDbContext _db;

    public MediaRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default) =>
        _db.MediaAssets.AddAsync(asset, cancellationToken).AsTask();
}
