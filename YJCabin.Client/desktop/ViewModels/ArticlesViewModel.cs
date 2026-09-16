using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class ArticlesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    [ObservableProperty] private ObservableCollection<ArticleSummary> _items = [];
    [ObservableProperty] private ArticleSummary? _selected;
    [ObservableProperty] private string _slug = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _markdown = "";
    [ObservableProperty] private string _tagsText = "";
    [ObservableProperty] private string _status = "Draft";
    [ObservableProperty] private string _message = "";

    public ArticlesViewModel(ApiClient api)
    {
        _api = api;
    }

    partial void OnSelectedChanged(ArticleSummary? value)
    {
        if (value is not null)
        {
            _ = LoadDetailAsync(value.Slug);
        }
    }

    [RelayCommand]
    public async Task ReloadAsync()
    {
        var result = await _api.ListArticlesAsync();
        Items = new ObservableCollection<ArticleSummary>(result.Items);
    }

    [RelayCommand]
    private void NewItem()
    {
        Selected = null;
        Slug = Title = Summary = Markdown = TagsText = "";
        Status = "Draft";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var saved = await _api.SaveArticleAsync(Selected?.Slug, new UpsertArticleRequest
            {
                Slug = string.IsNullOrWhiteSpace(Slug) ? null : Slug,
                Title = Title,
                Summary = Summary,
                Markdown = Markdown,
                Status = Status,
                Tags = TagsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            });
            Message = $"已保存 {saved.Slug}";
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Selected is null)
        {
            return;
        }

        await _api.DeleteArticleAsync(Selected.Slug);
        NewItem();
        await ReloadAsync();
    }

    private async Task LoadDetailAsync(string slug)
    {
        var detail = await _api.GetArticleAsync(slug);
        Slug = detail.Slug;
        Title = detail.Title;
        Summary = detail.Summary;
        Markdown = detail.Markdown;
        Status = detail.Status;
        TagsText = string.Join(", ", detail.Tags.Select(x => x.Name));
    }
}
