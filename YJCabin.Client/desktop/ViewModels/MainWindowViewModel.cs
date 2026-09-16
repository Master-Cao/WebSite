using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    [ObservableProperty] private bool _isAuthenticated;
    [ObservableProperty] private string _userName = "admin";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _status = "登录后管理作品、文章、关于我与留言。";
    [ObservableProperty] private int _selectedTab;
    [ObservableProperty] private ProjectsViewModel _projects;
    [ObservableProperty] private ArticlesViewModel _articles;
    [ObservableProperty] private AboutViewModel _about;
    [ObservableProperty] private MessagesViewModel _messages;

    public MainWindowViewModel(ApiClient api)
    {
        _api = api;
        Projects = new ProjectsViewModel(api);
        Articles = new ArticlesViewModel(api);
        About = new AboutViewModel(api);
        Messages = new MessagesViewModel(api);
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            await _api.LoginAsync(UserName, Password);
            IsAuthenticated = true;
            Status = "已登录";
            await Projects.ReloadAsync();
            await Articles.ReloadAsync();
            await About.ReloadAsync();
            await Messages.ReloadAsync();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _api.Logout();
        IsAuthenticated = false;
        Password = "";
        Status = "已退出，可重新登录。";
    }
}
