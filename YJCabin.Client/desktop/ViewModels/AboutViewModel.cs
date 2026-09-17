using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly BusyController _busy;
    private List<SocialLinkDto> _extraLinks = [];

    [ObservableProperty] private string _headline = "";
    [ObservableProperty] private string _bioMarkdown = "";
    [ObservableProperty] private string _qq = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _wechat = "";
    [ObservableProperty] private string _github = "";
    [ObservableProperty] private string _message = "";

    public AboutViewModel(ApiClient api, BusyController busy)
    {
        _api = api;
        _busy = busy;
    }

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载简介…", LoadCoreAsync);

    private async Task LoadCoreAsync()
    {
        var about = await _api.GetAboutAsync();
        Headline = about.Headline;
        BioMarkdown = about.BioMarkdown;
        Qq = Pick(about.SocialLinks, "qq", "腾讯");
        Email = StripMail(Pick(about.SocialLinks, "mail", "邮箱", "email"));
        Wechat = Pick(about.SocialLinks, "微信", "wechat", "weixin");
        Github = Pick(about.SocialLinks, "github");
        _extraLinks = about.SocialLinks
            .Where(link => !Matches(link, "qq", "腾讯", "mail", "邮箱", "email", "微信", "wechat", "weixin", "github"))
            .ToList();
        Message = "";
    }

    [RelayCommand]
    private Task SaveAsync() => _busy.RunAsync("正在保存简介…", SaveCoreAsync);

    private async Task SaveCoreAsync()
    {
        try
        {
            var links = new List<SocialLinkDto>(_extraLinks);
            AddLink(links, "QQ", Qq);
            AddLink(links, "邮箱", Email);
            AddLink(links, "微信", Wechat);
            AddLink(links, "GitHub", Github);

            var current = await _api.GetAboutAsync();
            await _api.SaveAboutAsync(new UpdateAboutRequest
            {
                Headline = Headline,
                BioMarkdown = BioMarkdown,
                Skills = current.Skills,
                SocialLinks = links
            });
            Message = "已保存";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private static string Pick(IEnumerable<SocialLinkDto> links, params string[] keys) =>
        links.FirstOrDefault(link => Matches(link, keys))?.Url?.Trim() ?? "";

    private static bool Matches(SocialLinkDto link, params string[] keys)
    {
        var hay = $"{link.Name} {link.Url}";
        return keys.Any(key => hay.Contains(key, StringComparison.OrdinalIgnoreCase));
    }

    private static string StripMail(string value) =>
        value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ? value[7..] : value;

    private static void AddLink(List<SocialLinkDto> links, string name, string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        links.Add(new SocialLinkDto { Name = name, Url = trimmed });
    }
}
