using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly BusyController _busy;

    [ObservableProperty] private ObservableCollection<TagDto> _tags = [];
    [ObservableProperty] private ObservableCollection<string> _skills = [];
    [ObservableProperty] private string _newTagName = "";
    [ObservableProperty] private string _newSkillName = "";
    [ObservableProperty] private string _message = "";

    public SettingsViewModel(ApiClient api, BusyController busy)
    {
        _api = api;
        _busy = busy;
    }

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载设置…", LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        var tags = await _api.ListTagsAsync();
        var about = await _api.GetAboutAsync();
        Tags = new ObservableCollection<TagDto>(tags.OrderBy(x => x.Name));
        Skills = new ObservableCollection<string>(about.Skills);
        Message = "";
    }

    [RelayCommand]
    private Task AddTag() => _busy.RunAsync("正在添加标签…", async () =>
    {
        var name = NewTagName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            Message = "请输入标签名称。";
            return;
        }

        try
        {
            var created = await _api.CreateTagAsync(name);
            Tags.Add(created);
            NewTagName = "";
            Message = $"已添加标签「{created.Name}」。";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    });

    [RelayCommand]
    private Task RemoveTag(TagDto? tag) => _busy.RunAsync("正在删除标签…", async () =>
    {
        if (tag is null)
        {
            return;
        }

        try
        {
            await _api.DeleteTagAsync(tag.Id);
            Tags.Remove(tag);
            Message = $"已删除标签「{tag.Name}」。";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    });

    [RelayCommand]
    private Task AddSkill() => _busy.RunAsync("正在添加技能…", async () =>
    {
        var name = NewSkillName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            Message = "请输入技能名称。";
            return;
        }

        if (Skills.Any(item => item.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            Message = $"技能「{name}」已存在。";
            return;
        }

        Skills.Add(name);
        NewSkillName = "";
        await SaveSkillsAsync($"已添加技能「{name}」。");
    });

    [RelayCommand]
    private Task RemoveSkill(string? name) => _busy.RunAsync("正在删除技能…", async () =>
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var match = Skills.FirstOrDefault(item => item == name);
        if (match is null)
        {
            return;
        }

        Skills.Remove(match);
        await SaveSkillsAsync($"已删除技能「{name}」。");
    });

    private async Task SaveSkillsAsync(string ok)
    {
        try
        {
            var about = await _api.GetAboutAsync();
            await _api.SaveAboutAsync(new UpdateAboutRequest
            {
                Headline = about.Headline,
                BioMarkdown = about.BioMarkdown,
                Skills = Skills.ToList(),
                SocialLinks = about.SocialLinks
            });
            Message = ok;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            await LoadCoreAsync();
        }
    }
}
