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
    [ObservableProperty] private string _aiBaseUrl = "";
    [ObservableProperty] private string _aiModel = "";
    [ObservableProperty] private string _aiApiKey = "";
    [ObservableProperty] private string _aiMessage = "";

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
        var ai = AiSettings.Load();
        AiBaseUrl = ai.BaseUrl;
        AiModel = ai.Model;
        AiApiKey = ai.ApiKey;
        Message = "";
    }

    [RelayCommand]
    private void SaveAiSettings()
    {
        try
        {
            var settings = new AiSettings
            {
                BaseUrl = string.IsNullOrWhiteSpace(AiBaseUrl) ? AiSettings.DefaultBaseUrl : AiBaseUrl.Trim(),
                Model = string.IsNullOrWhiteSpace(AiModel) ? AiSettings.DefaultModel : AiModel.Trim(),
                ApiKey = AiApiKey.Trim()
            };
            settings.Save();
            AiBaseUrl = settings.BaseUrl;
            AiModel = settings.Model;
            if (settings.IsConfigured)
            {
                ToastSuccess("Kimi 设置已保存。");
            }
            else
            {
                ToastWarn("已保存，但还缺少 API Key，生成摘要前请补全。");
            }
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    }

    [RelayCommand]
    private Task AddTag() => _busy.RunAsync("正在添加标签…", async () =>
    {
        var name = NewTagName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ToastWarn("请输入标签名称。");
            return;
        }

        try
        {
            var created = await _api.CreateTagAsync(name);
            Tags.Add(created);
            NewTagName = "";
            ToastSuccess($"已添加标签「{created.Name}」。");
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
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
            ToastSuccess($"已删除标签「{tag.Name}」。");
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    });

    [RelayCommand]
    private Task AddSkill() => _busy.RunAsync("正在添加技能…", async () =>
    {
        var name = NewSkillName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ToastWarn("请输入技能名称。");
            return;
        }

        if (Skills.Any(item => item.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            ToastWarn($"技能「{name}」已存在。");
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
            ToastSuccess(ok);
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
            await LoadCoreAsync();
        }
    }
}
