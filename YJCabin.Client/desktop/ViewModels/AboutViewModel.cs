using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    [ObservableProperty] private string _headline = "";
    [ObservableProperty] private string _bioMarkdown = "";
    [ObservableProperty] private string _skillsText = "";
    [ObservableProperty] private string _socialText = "";
    [ObservableProperty] private string _message = "";

    public AboutViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    public async Task ReloadAsync()
    {
        var about = await _api.GetAboutAsync();
        Headline = about.Headline;
        BioMarkdown = about.BioMarkdown;
        SkillsText = string.Join(", ", about.Skills);
        SocialText = string.Join(Environment.NewLine, about.SocialLinks.Select(x => $"{x.Name}|{x.Url}"));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            await _api.SaveAboutAsync(new UpdateAboutRequest
            {
                Headline = Headline,
                BioMarkdown = BioMarkdown,
                Skills = SkillsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
                SocialLinks = SocialText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(line =>
                    {
                        var parts = line.Split('|');
                        return new SocialLinkDto
                        {
                            Name = parts.ElementAtOrDefault(0)?.Trim() ?? "",
                            Url = parts.ElementAtOrDefault(1)?.Trim() ?? ""
                        };
                    }).ToList()
            });
            Message = "已保存";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }
}
