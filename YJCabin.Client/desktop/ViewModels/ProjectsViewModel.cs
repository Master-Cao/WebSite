using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class ProjectsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    [ObservableProperty] private ObservableCollection<ProjectSummary> _items = [];
    [ObservableProperty] private ProjectSummary? _selected;
    [ObservableProperty] private string _slug = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _tagsText = "";
    [ObservableProperty] private string _status = "Draft";
    [ObservableProperty] private string _message = "";

    public ProjectsViewModel(ApiClient api)
    {
        _api = api;
    }

    partial void OnSelectedChanged(ProjectSummary? value)
    {
        if (value is null)
        {
            return;
        }

        _ = LoadDetailAsync(value.Slug);
    }

    [RelayCommand]
    public async Task ReloadAsync()
    {
        var result = await _api.ListProjectsAsync();
        Items = new ObservableCollection<ProjectSummary>(result.Items);
    }

    [RelayCommand]
    private void NewItem()
    {
        Selected = null;
        Slug = Title = Summary = Description = TagsText = "";
        Status = "Draft";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var currentSlug = Selected?.Slug;
            var saved = await _api.SaveProjectAsync(currentSlug, new UpsertProjectRequest
            {
                Slug = string.IsNullOrWhiteSpace(Slug) ? null : Slug,
                Title = Title,
                Summary = Summary,
                Description = Description,
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

        await _api.DeleteProjectAsync(Selected.Slug);
        NewItem();
        await ReloadAsync();
    }

    private async Task LoadDetailAsync(string slug)
    {
        var detail = await _api.GetProjectAsync(slug);
        Slug = detail.Slug;
        Title = detail.Title;
        Summary = detail.Summary;
        Description = detail.Description;
        Status = detail.Status;
        TagsText = string.Join(", ", detail.Tags.Select(x => x.Name));
    }
}
