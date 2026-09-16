using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Options;
using YJCabin.Domain.Common;
using YJCabin.Domain.Entities;
using YJCabin.Infrastructure.Persistence;

namespace YJCabin.Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IArticleContentStore _articles;
    private readonly SeedOptions _seed;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        IArticleContentStore articles,
        IOptions<SeedOptions> seed,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _articles = articles;
        _seed = seed.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.MigrateAsync(cancellationToken);

        if (!await _db.Users.AnyAsync(cancellationToken))
        {
            await _db.Users.AddAsync(new User
            {
                UserName = _seed.AdminUserName,
                Email = _seed.AdminEmail,
                PasswordHash = _passwordHasher.Hash(_seed.AdminPassword),
                Role = "Admin"
            }, cancellationToken);
            _logger.LogInformation("Seeded admin user {UserName}", _seed.AdminUserName);
        }

        if (!await _db.Tags.AnyAsync(cancellationToken))
        {
            _db.Tags.AddRange(
                new Tag { Name = "Architecture", Slug = "architecture" },
                new Tag { Name = "C#", Slug = "csharp" },
                new Tag { Name = "React", Slug = "react" });
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (!await _db.Projects.AnyAsync(cancellationToken))
        {
            var tags = await _db.Tags.ToListAsync(cancellationToken);
            var project = new Project
            {
                Slug = "yjcabin",
                Title = "YJCabin",
                Summary = "个人作品与文章站点，Web 与 Windows 桌面共用同一套 API。",
                Description = "YJCabin 是一套前后端分离的个人站点：React 展示站、Avalonia 管理端、ASP.NET Core Web API 与 PostgreSQL。文章以 Markdown 为真源，作品元数据入库。",
                CoverUrl = null,
                RepoUrl = null,
                LiveUrl = null,
                Status = ContentStatus.Published,
                SortOrder = 1,
                PublishedAt = DateTimeOffset.UtcNow
            };
            foreach (var tag in tags)
            {
                project.ProjectTags.Add(new ProjectTag { Project = project, Tag = tag });
            }

            await _db.Projects.AddAsync(project, cancellationToken);
        }

        if (!await _db.AboutPages.AnyAsync(cancellationToken))
        {
            await _db.AboutPages.AddAsync(new AboutPage
            {
                Headline = "在小屋里写代码、做作品。",
                BioMarkdown = "你好，我是 YJCabin 的主人。这里用来展示我的项目，以及一些关于架构、工程与产品的短文。",
                SkillsJson = JsonSerializer.Serialize(new[] { "C#", "React", "Avalonia", "PostgreSQL" }),
                SocialLinksJson = JsonSerializer.Serialize(new[]
                {
                    new { name = "GitHub", url = "https://github.com" }
                })
            }, cancellationToken);
        }

        if (!await _db.Articles.AnyAsync(cancellationToken))
        {
            var document = new ArticleDocument
            {
                Slug = "hello-yjcabin",
                Title = "Hello, YJCabin",
                Summary = "站点第一篇文章：为什么用混合存储，以及 Web / 桌面如何共用 API。",
                CoverUrl = null,
                PublishedAt = DateTimeOffset.UtcNow,
                Draft = false,
                Tags = ["Architecture", "C#"],
                MarkdownBody = """
站点采用混合内容模型：

- 文章正文存在 Markdown 文件中，便于 Git 与本地编辑。
- 标题、摘要、标签等元数据写入 PostgreSQL，方便筛选与分页。
- React 展示站与 Avalonia 管理端都只调用同一套 REST API。
""",
                RawFile = string.Empty,
                Hash = string.Empty,
                RelativePath = "articles/hello-yjcabin.md"
            };
            document = await _articles.WriteAsync(document, cancellationToken);

            var article = new Article
            {
                Slug = document.Slug,
                Title = document.Title,
                Summary = document.Summary,
                Status = ContentStatus.Published,
                PublishedAt = document.PublishedAt,
                FilePath = document.RelativePath,
                ContentHash = document.Hash
            };
            var architecture = await _db.Tags.FirstAsync(x => x.Slug == "architecture", cancellationToken);
            var csharp = await _db.Tags.FirstAsync(x => x.Slug == "csharp", cancellationToken);
            article.ArticleTags.Add(new ArticleTag { Article = article, Tag = architecture });
            article.ArticleTags.Add(new ArticleTag { Article = article, Tag = csharp });
            await _db.Articles.AddAsync(article, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
