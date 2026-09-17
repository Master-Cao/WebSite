using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class ArticlesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly BusyController _busy;

    [ObservableProperty] private ObservableCollection<ArticleSummary> _items = [];
    [ObservableProperty] private ArticleSummary? _selected;
    [ObservableProperty] private bool _isCreating;
    [ObservableProperty] private string _slug = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _markdown = "";
    [ObservableProperty] private ObservableCollection<TagChoice> _tagChoices = [];
    [ObservableProperty] private string _status = "Draft";
    [ObservableProperty] private string _message = "";

    public bool HasItems => Items.Count > 0;
    public bool ShowEditor => IsCreating || Selected is not null;
    public bool CanDelete => Selected is not null;
    public bool HasMarkdown => !string.IsNullOrWhiteSpace(Markdown);
    public string EditorTitle => IsCreating ? "新建文章" : "编辑文章";
    public string SlugHint => IsCreating || string.IsNullOrWhiteSpace(Slug)
        ? "保存后会根据标题自动生成网址"
        : $"/articles/{Slug}";

    public ArticlesViewModel(ApiClient api, BusyController busy)
    {
        _api = api;
        _busy = busy;
    }

    partial void OnItemsChanged(ObservableCollection<ArticleSummary> value) =>
        OnPropertyChanged(nameof(HasItems));

    partial void OnSelectedChanged(ArticleSummary? value)
    {
        NotifyEditor();
        if (value is null)
        {
            return;
        }

        IsCreating = false;
        _ = LoadDetailAsync(value.Slug);
    }

    partial void OnIsCreatingChanged(bool value) => NotifyEditor();

    partial void OnSlugChanged(string value) => OnPropertyChanged(nameof(SlugHint));

    partial void OnMarkdownChanged(string value) => OnPropertyChanged(nameof(HasMarkdown));

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载文章…", LoadListAsync);

    private async Task LoadListAsync()
    {
        var result = await _api.ListArticlesAsync();
        var slug = Selected?.Slug;
        Items = new ObservableCollection<ArticleSummary>(result.Items);
        Selected = slug is null ? null : Items.FirstOrDefault(item => item.Slug == slug);
        if (Selected is null && !IsCreating)
        {
            Message = Items.Count == 0 ? "还没有文章，点击新建开始。" : "";
        }
    }

    [RelayCommand]
    private Task NewItem() => _busy.RunAsync("正在准备编辑器…", StartNewAsync);

    private async Task StartNewAsync()
    {
        IsCreating = true;
        Selected = null;
        Slug = Title = Summary = Markdown = "";
        Status = "Draft";
        Message = "";
        await LoadTagChoicesAsync([]);
    }

    [RelayCommand]
    private Task SaveAsync() => _busy.RunAsync("正在保存文章…", () => SaveCoreAsync(false));

    [RelayCommand]
    private Task PublishAsync() => _busy.RunAsync("正在发布文章…", () => SaveCoreAsync(true));

    private async Task SaveCoreAsync(bool publish)
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Markdown))
        {
            Message = "请填写标题和正文后再保存。";
            return;
        }

        try
        {
            if (publish)
            {
                Status = "Published";
            }

            var currentSlug = IsCreating ? null : Selected?.Slug;
            var saved = await _api.SaveArticleAsync(currentSlug, new UpsertArticleRequest
            {
                Slug = null,
                Title = Title.Trim(),
                Summary = Summary,
                Markdown = Markdown,
                Status = Status,
                Tags = TagCatalog.SelectedNames(TagChoices)
            });
            IsCreating = false;
            Slug = saved.Slug;
            if (publish || saved.Status == "Published")
            {
                await ReturnToListAsync($"已发布「{saved.Title}」。");
                return;
            }

            Message = "已保存草稿。";
            var result = await _api.ListArticlesAsync();
            Items = new ObservableCollection<ArticleSummary>(result.Items);
            Selected = Items.FirstOrDefault(item => item.Slug == saved.Slug);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private Task DeleteAsync() => _busy.RunAsync("正在删除文章…", DeleteCoreAsync);

    private async Task DeleteCoreAsync()
    {
        if (Selected is null)
        {
            return;
        }

        await _api.DeleteArticleAsync(Selected.Slug);
        IsCreating = false;
        Selected = null;
        Slug = Title = Summary = Markdown = "";
        TagChoices = [];
        Status = "Draft";
        Message = "已删除";
        await LoadListAsync();
    }

    private async Task ReturnToListAsync(string message)
    {
        IsCreating = false;
        Selected = null;
        var result = await _api.ListArticlesAsync();
        Items = new ObservableCollection<ArticleSummary>(result.Items);
        Message = Items.Count == 0 ? "还没有文章，点击新建开始。" : message;
    }

    [RelayCommand]
    private void OpenItem(ArticleSummary? item)
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
        Message = "";
    }

    private void NotifyEditor()
    {
        OnPropertyChanged(nameof(ShowEditor));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SlugHint));
    }

    private Task LoadDetailAsync(string slug) => _busy.RunAsync("正在打开文章…", async () =>
    {
        var detail = await _api.GetArticleAsync(slug);
        Slug = detail.Slug;
        Title = detail.Title;
        Summary = detail.Summary;
        Markdown = detail.Markdown;
        Status = detail.Status;
        await LoadTagChoicesAsync(detail.Tags.Select(x => x.Name));
    });

    private async Task LoadTagChoicesAsync(IEnumerable<string> selected)
    {
        TagChoices = await TagCatalog.LoadAsync(_api, selected);
    }
}
