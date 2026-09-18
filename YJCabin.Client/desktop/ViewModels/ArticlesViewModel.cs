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
    [ObservableProperty] private bool _isImmersive;
    private readonly HashSet<string> _unsavedLocalImages = new(StringComparer.OrdinalIgnoreCase);

    public bool HasItems => Items.Count > 0;
    public bool ShowEditor => IsCreating || Selected is not null;
    public bool CanDelete => Selected is not null;
    public bool HasMarkdown => !string.IsNullOrWhiteSpace(Markdown);
    public string EditorTitle => IsCreating ? "新建文章" : "编辑文章";
    public string ImmersiveButtonText => IsImmersive ? "退出沉浸" : "沉浸式编写";
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

    partial void OnIsImmersiveChanged(bool value) => OnPropertyChanged(nameof(ImmersiveButtonText));

    partial void OnSlugChanged(string value) => OnPropertyChanged(nameof(SlugHint));

    partial void OnMarkdownChanged(string value)
    {
        OnPropertyChanged(nameof(HasMarkdown));
        OnPropertyChanged(nameof(PreviewMarkdown));
    }

    public string MediaBaseUrl => _api.BaseUrl.TrimEnd('/') + "/";

    public string PreviewMarkdown
    {
        get
        {
            var root = _api.BaseUrl.TrimEnd('/');
            var text = Markdown ?? "";
            text = text.Replace($"]({root}/uploads/", $"]({root}/api/uploads/", StringComparison.Ordinal);
            text = text.Replace("](/uploads/", $"]({root}/api/uploads/", StringComparison.Ordinal);
            text = text.Replace("](/api/uploads/", $"]({root}/api/uploads/", StringComparison.Ordinal);
            return text;
        }
    }

    [RelayCommand]
    private Task SummarizeAsync() => _busy.RunAsync("正在生成摘要…", SummarizeCoreAsync);

    private async Task SummarizeCoreAsync()
    {
        if (string.IsNullOrWhiteSpace(Markdown))
        {
            ToastWarn("请先写正文，再生成摘要。");
            return;
        }

        try
        {
            Summary = await AiSummaryClient.SummarizeArticleAsync(Title, Markdown);
            ToastSuccess("摘要已生成，可再手动修改。");
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    }

    public async Task<int> InsertImageAsync(int caret, Stream content, string fileName, string contentType)
    {
        try
        {
            var id = await LocalMediaStore.SaveAsync(content, fileName);
            _unsavedLocalImages.Add(id);
            var snippet = $"\n![图片]({LocalMediaStore.ToUrl(id)})\n";
            var text = Markdown ?? "";
            caret = Math.Clamp(caret, 0, text.Length);
            Markdown = text.Insert(caret, snippet);
            ToastInfo("图片已放入本地缓存，发布时才会上传。");
            return caret + snippet.Length;
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
            return caret;
        }
    }

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载文章…", LoadListAsync);

    private async Task LoadListAsync()
    {
        var result = await _api.ListArticlesAsync();
        var slug = Selected?.Slug;
        Items = new ObservableCollection<ArticleSummary>(result.Items);
        Selected = slug is null ? null : Items.FirstOrDefault(item => item.Slug == slug);
    }

    [RelayCommand]
    private Task NewItem() => _busy.RunAsync("正在准备编辑器…", StartNewAsync);

    private async Task StartNewAsync()
    {
        DiscardUnsavedLocalImages();
        IsCreating = true;
        Selected = null;
        Slug = Title = Summary = Markdown = "";
        Status = "Draft";
        await LoadTagChoicesAsync([]);
    }

    [RelayCommand]
    private Task SaveAsync() => _busy.RunAsync("正在保存文章…", () => SaveCoreAsync(false));

    [RelayCommand]
    private Task PublishAsync() => _busy.RunAsync(
        LocalMediaStore.ContainsLocal(Markdown) ? "正在上传图片并发布…" : "正在发布文章…",
        () => SaveCoreAsync(true));

    private async Task SaveCoreAsync(bool publish)
    {
        if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Markdown))
        {
            ToastWarn("请填写标题和正文后再保存。");
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ToastWarn("请填写标题后再保存。");
            return;
        }

        if (string.IsNullOrWhiteSpace(Markdown))
        {
            ToastWarn("请填写正文后再保存。");
            return;
        }

        try
        {
            if (publish)
            {
                Status = "Published";
            }

            var markdown = Markdown;
            var uploaded = Array.Empty<string>();
            if (publish)
            {
                uploaded = LocalMediaStore.ListIds(markdown).ToArray();
                markdown = await LocalMediaStore.UploadAndRewriteAsync(markdown, _api.UploadMediaAsync);
                Markdown = markdown;
            }

            var currentSlug = IsCreating ? null : Selected?.Slug;
            var saved = await _api.SaveArticleAsync(currentSlug, new UpsertArticleRequest
            {
                Slug = null,
                Title = Title.Trim(),
                Summary = Summary,
                Markdown = markdown,
                Status = Status,
                Tags = TagCatalog.SelectedNames(TagChoices)
            });
            if (publish)
            {
                LocalMediaStore.DeleteAll(uploaded);
            }

            _unsavedLocalImages.Clear();
            IsCreating = false;
            Slug = saved.Slug;
            if (publish || saved.Status == "Published")
            {
                await ReturnToListAsync();
                ToastSuccess($"已发布「{saved.Title}」。");
                return;
            }

            ToastSuccess("已保存草稿。");
            var result = await _api.ListArticlesAsync();
            Items = new ObservableCollection<ArticleSummary>(result.Items);
            Selected = Items.FirstOrDefault(item => item.Slug == saved.Slug);
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
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
        LocalMediaStore.DeleteAll(LocalMediaStore.ListIds(Markdown).Concat(_unsavedLocalImages));
        _unsavedLocalImages.Clear();
        IsCreating = false;
        Selected = null;
        IsImmersive = false;
        Slug = Title = Summary = Markdown = "";
        TagChoices = [];
        Status = "Draft";
        ToastSuccess("文章已删除。");
        await LoadListAsync();
    }

    private async Task ReturnToListAsync()
    {
        IsCreating = false;
        Selected = null;
        IsImmersive = false;
        var result = await _api.ListArticlesAsync();
        Items = new ObservableCollection<ArticleSummary>(result.Items);
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
    private void ToggleImmersive() => IsImmersive = !IsImmersive;

    public void ExitImmersive() => IsImmersive = false;

    [RelayCommand]
    private void CloseEditor()
    {
        DiscardUnsavedLocalImages();
        IsImmersive = false;
        IsCreating = false;
        Selected = null;
        Message = "";
    }

    private void DiscardUnsavedLocalImages()
    {
        LocalMediaStore.DeleteAll(_unsavedLocalImages);
        _unsavedLocalImages.Clear();
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
        DiscardUnsavedLocalImages();
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
