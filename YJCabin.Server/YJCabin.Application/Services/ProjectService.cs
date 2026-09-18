using YJCabin.Application.Abstractions;
using YJCabin.Application.Common;
using YJCabin.Application.Dtos;
using YJCabin.Application.Mapping;
using YJCabin.Domain.Common;
using YJCabin.Domain.Entities;

namespace YJCabin.Application.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ITagRepository _tags;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IProjectRepository projects, ITagRepository tags, IUnitOfWork unitOfWork)
    {
        _projects = projects;
        _tags = tags;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProjectSummaryDto>> ListAsync(ListQuery query, bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _projects.ListAsync(query, publishedOnly, cancellationToken);
        return new PagedResult<ProjectSummaryDto>
        {
            Items = items.Select(x => x.ToSummary()).ToList(),
            Page = query.SafePage,
            PageSize = query.SafePageSize,
            TotalCount = total
        };
    }

    public async Task<ProjectDetailDto> GetBySlugAsync(string slug, bool publishedOnly, CancellationToken cancellationToken = default)
    {
        var project = await FindAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Project", slug);
        if (publishedOnly && project.Status != ContentStatus.Published)
        {
            throw new NotFoundException("Project", slug);
        }

        return project.ToDetail();
    }

    private async Task<Project?> FindAsync(string key, CancellationToken cancellationToken)
    {
        if (!SlugHelper.IsUsable(key))
        {
            return null;
        }

        var project = await _projects.GetBySlugAsync(key, cancellationToken);
        if (project is not null)
        {
            return project;
        }

        return Guid.TryParse(key, out var id)
            ? await _projects.GetByIdAsync(id, cancellationToken)
            : null;
    }

    public async Task<ProjectDetailDto> CreateAsync(UpsertProjectRequest request, CancellationToken cancellationToken = default)
    {
        var slug = SlugHelper.From(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        if (await _projects.SlugExistsAsync(slug, null, cancellationToken))
        {
            throw new ConflictException($"Project slug '{slug}' already exists.");
        }

        var project = new Project { Slug = slug };
        await ApplyAsync(project, request, slug, cancellationToken);
        await _projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await _projects.GetBySlugAsync(slug, cancellationToken))!.ToDetail();
    }

    public async Task<ProjectDetailDto> UpdateAsync(string slug, UpsertProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetBySlugAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Project", slug);
        var nextSlug = SlugHelper.From(string.IsNullOrWhiteSpace(request.Slug) ? request.Title : request.Slug);
        if (nextSlug != project.Slug && await _projects.SlugExistsAsync(nextSlug, project.Id, cancellationToken))
        {
            throw new ConflictException($"Project slug '{nextSlug}' already exists.");
        }

        await ApplyAsync(project, request, nextSlug, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await _projects.GetBySlugAsync(project.Slug, cancellationToken))!.ToDetail();
    }

    public async Task DeleteAsync(string slug, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetBySlugAsync(slug, cancellationToken)
                      ?? throw new NotFoundException("Project", slug);
        _projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyAsync(Project project, UpsertProjectRequest request, string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Summary))
        {
            throw new ValidationException("title", "标题和摘要不能为空。");
        }

        project.Slug = slug;
        project.Title = request.Title.Trim();
        project.Summary = request.Summary.Trim();
        project.Description = request.Description?.Trim() ?? string.Empty;
        project.CoverUrl = request.CoverUrl;
        project.RepoUrl = request.RepoUrl;
        project.LiveUrl = request.LiveUrl;
        project.Status = request.Status;
        project.SortOrder = request.SortOrder;
        project.PublishedAt = DtoMapper.ResolvePublishedAt(request.Status, request.PublishedAt);
        project.UpdatedAt = DateTimeOffset.UtcNow;

        var tags = await _tags.GetOrCreateManyAsync(request.Tags, cancellationToken);
        project.ProjectTags.Clear();
        foreach (var tag in tags)
        {
            project.ProjectTags.Add(new ProjectTag { Project = project, Tag = tag });
        }

        project.Images.Clear();
        foreach (var image in request.Images.OrderBy(x => x.SortOrder))
        {
            project.Images.Add(new ProjectImage
            {
                Url = image.Url,
                Caption = image.Caption,
                SortOrder = image.SortOrder
            });
        }
    }
}
