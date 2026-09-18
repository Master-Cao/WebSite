using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class ProjectsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly BusyController _busy;

    [ObservableProperty] private ObservableCollection<ProjectSummary> _items = [];
    [ObservableProperty] private ProjectSummary? _selected;
    [ObservableProperty] private bool _isCreating;
    [ObservableProperty] private string _slug = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private ObservableCollection<TagChoice> _tagChoices = [];
    [ObservableProperty] private string _status = "Draft";
    [ObservableProperty] private string _message = "";

    public IReadOnlyList<string> StatusOptions { get; } = ["Draft", "Published", "Archived"];
    public bool HasItems => Items.Count > 0;
    public bool ShowEditor => IsCreating || Selected is not null;
    public bool CanDelete => Selected is not null;
    public string EditorTitle => IsCreating ? "新建作品" : "编辑作品";

    public ProjectsViewModel(ApiClient api, BusyController busy)
    {
        _api = api;
        _busy = busy;
    }

    partial void OnItemsChanged(ObservableCollection<ProjectSummary> value) =>
        OnPropertyChanged(nameof(HasItems));

    partial void OnSelectedChanged(ProjectSummary? value)
    {
        NotifyEditor();
        if (value is null)
        {
            return;
        }

        IsCreating = false;
        _ = string.IsNullOrWhiteSpace(value.Slug) ? Task.CompletedTask : LoadDetailAsync(value.Slug);
    }

    partial void OnIsCreatingChanged(bool value) => NotifyEditor();

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载作品…", LoadListAsync);

    private async Task LoadListAsync()
    {
        var result = await _api.ListProjectsAsync();
        var slug = Selected?.Slug;
        Items = new ObservableCollection<ProjectSummary>(result.Items);
        Selected = slug is null ? null : Items.FirstOrDefault(item => item.Slug == slug);
    }

    [RelayCommand]
    private Task NewItem() => _busy.RunAsync("正在准备编辑器…", async () =>
    {
        IsCreating = true;
        Selected = null;
        Slug = Title = Summary = Description = "";
        Status = "Draft";
        TagChoices = await TagCatalog.LoadAsync(_api, []);
    });

    [RelayCommand]
    private Task SaveAsync() => _busy.RunAsync("正在保存作品…", () => SaveCoreAsync(false));

    [RelayCommand]
    private Task PublishAsync() => _busy.RunAsync("正在发布作品…", () => SaveCoreAsync(true));

    private async Task SaveCoreAsync(bool publish)
    {
        if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Summary))
        {
            ToastWarn("请填写标题和摘要后再保存。");
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ToastWarn("请填写标题后再保存。");
            return;
        }

        if (string.IsNullOrWhiteSpace(Summary))
        {
            ToastWarn("请填写摘要后再保存。");
            return;
        }

        try
        {
            if (publish)
            {
                Status = "Published";
            }

            var currentSlug = IsCreating ? null : Selected?.Slug;
            var saved = await _api.SaveProjectAsync(currentSlug, new UpsertProjectRequest
            {
                Slug = string.IsNullOrWhiteSpace(Slug) ? null : Slug,
                Title = Title,
                Summary = Summary,
                Description = Description,
                Status = Status,
                Tags = TagCatalog.SelectedNames(TagChoices)
            });
            IsCreating = false;
            if (publish || saved.Status == "Published")
            {
                await ReturnToListAsync();
                ToastSuccess($"已发布「{saved.Title}」。");
                return;
            }

            ToastSuccess("已保存草稿。");
            var result = await _api.ListProjectsAsync();
            Items = new ObservableCollection<ProjectSummary>(result.Items);
            Selected = Items.FirstOrDefault(item => item.Slug == saved.Slug);
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    }

    [RelayCommand]
    private Task DeleteAsync() => _busy.RunAsync("正在删除作品…", DeleteCoreAsync);

    private async Task DeleteCoreAsync()
    {
        if (Selected is null)
        {
            return;
        }

        await _api.DeleteProjectAsync(Selected.Slug);
        IsCreating = false;
        Selected = null;
        Slug = Title = Summary = Description = "";
        TagChoices = [];
        Status = "Draft";
        ToastSuccess("作品已删除。");
        await LoadListAsync();
    }

    private async Task ReturnToListAsync()
    {
        IsCreating = false;
        Selected = null;
        var result = await _api.ListProjectsAsync();
        Items = new ObservableCollection<ProjectSummary>(result.Items);
    }

    [RelayCommand]
    private void OpenItem(ProjectSummary? item)
    {
        if (item is null)
        {
            return;
        }

        Selected = item;
    }

    [RelayCommand]
    private void CloseEditor()
    {
        IsCreating = false;
        Selected = null;
    }

    private void NotifyEditor()
    {
        OnPropertyChanged(nameof(ShowEditor));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(EditorTitle));
    }

    private Task LoadDetailAsync(string slug) => _busy.RunAsync("正在打开作品…", async () =>
    {
        var detail = await _api.GetProjectAsync(slug);
        Slug = detail.Slug;
        Title = detail.Title;
        Summary = detail.Summary;
        Description = detail.Description;
        Status = detail.Status;
        TagChoices = await TagCatalog.LoadAsync(_api, detail.Tags.Select(x => x.Name));
    });
}
