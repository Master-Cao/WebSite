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
    [ObservableProperty] private string _status = "已登录";
    [ObservableProperty] private string _loginError = "";
    [ObservableProperty] private string _siteName = "YJCabin";
    [ObservableProperty] private bool _isChangePasswordOpen;
    [ObservableProperty] private string _currentPassword = "";
    [ObservableProperty] private string _newPassword = "";
    [ObservableProperty] private string _confirmPassword = "";
    [ObservableProperty] private string _passwordMessage = "";
    [ObservableProperty] private bool _passwordSucceeded;
    [ObservableProperty] private NavItem? _selectedNav;
    [ObservableProperty] private ViewModelBase? _currentPage;
    [ObservableProperty] private ProjectsViewModel _projects;
    [ObservableProperty] private ArticlesViewModel _articles;
    [ObservableProperty] private AboutViewModel _about;
    [ObservableProperty] private MessagesViewModel _messages;
    [ObservableProperty] private SettingsViewModel _settings;

    public BusyController Busy { get; } = new();

    public IReadOnlyList<NavItem> NavItems { get; } =
    [
        new("projects", "作品", "管理公开作品"),
        new("articles", "文章", "撰写与发布"),
        new("about", "关于我", "简介与联系方式"),
        new("messages", "留言", "查看访客留言"),
        new("settings", "设置", "标签、技能与 AI")
    ];

    public bool HasLoginError => !string.IsNullOrWhiteSpace(LoginError);
    public bool HasPasswordMessage => !string.IsNullOrWhiteSpace(PasswordMessage);
    public bool HasPasswordError => HasPasswordMessage && !PasswordSucceeded;
    public bool IsImmersiveWriting =>
        IsAuthenticated && Articles.IsImmersive && SelectedNav?.Key == "articles";

    public MainWindowViewModel(ApiClient api)
    {
        _api = api;
        Projects = new ProjectsViewModel(api, Busy);
        Articles = new ArticlesViewModel(api, Busy);
        About = new AboutViewModel(api, Busy);
        Messages = new MessagesViewModel(api, Busy);
        Settings = new SettingsViewModel(api, Busy);
        SelectedNav = NavItems[0];
        Articles.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(ArticlesViewModel.IsImmersive) or null)
            {
                OnPropertyChanged(nameof(IsImmersiveWriting));
            }
        };
        About.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(AboutViewModel.Headline) or null)
            {
                RefreshSiteName();
            }
        };
        Busy.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(BusyController.IsBusy) or null)
            {
                LoginCommand.NotifyCanExecuteChanged();
                ChangePasswordCommand.NotifyCanExecuteChanged();
            }
        };
    }

    partial void OnSelectedNavChanged(NavItem? value)
    {
        CurrentPage = value?.Key switch
        {
            "projects" => Projects,
            "articles" => Articles,
            "about" => About,
            "messages" => Messages,
            "settings" => Settings,
            _ => null
        };
        if (value?.Key != "articles")
        {
            Articles.ExitImmersive();
        }

        OnPropertyChanged(nameof(IsImmersiveWriting));
        if (IsAuthenticated && value is not null)
        {
            Status = $"当前模块：{value.Title}";
        }
    }

    partial void OnIsAuthenticatedChanged(bool value) => OnPropertyChanged(nameof(IsImmersiveWriting));

    partial void OnUserNameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();

    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();

    partial void OnLoginErrorChanged(string value) => OnPropertyChanged(nameof(HasLoginError));

    partial void OnCurrentPasswordChanged(string value) => ChangePasswordCommand.NotifyCanExecuteChanged();

    partial void OnNewPasswordChanged(string value) => ChangePasswordCommand.NotifyCanExecuteChanged();

    partial void OnConfirmPasswordChanged(string value) => ChangePasswordCommand.NotifyCanExecuteChanged();

    partial void OnPasswordMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasPasswordMessage));
        OnPropertyChanged(nameof(HasPasswordError));
    }

    partial void OnPasswordSucceededChanged(bool value) => OnPropertyChanged(nameof(HasPasswordError));

    private bool CanLogin() =>
        !Busy.IsBusy &&
        !string.IsNullOrWhiteSpace(UserName) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        LoginError = "";
        try
        {
            await Busy.RunAsync("正在登录…", () => _api.LoginAsync(UserName.Trim(), Password));
        IsAuthenticated = true;
        SelectedNav ??= NavItems[0];
        Status = $"当前模块：{SelectedNav.Title}";
        OnPropertyChanged(nameof(IsImmersiveWriting));
            await Task.WhenAll(
                Projects.ReloadAsync(),
                Articles.ReloadAsync(),
                About.ReloadAsync(),
                Messages.ReloadAsync(),
                Settings.ReloadAsync());
            RefreshSiteName();
        }
        catch (Exception ex)
        {
            LoginError = FriendlyLoginError(ex.Message);
            ToastError(LoginError);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        _api.Logout();
        IsAuthenticated = false;
        Articles.ExitImmersive();
        Password = "";
        LoginError = "";
        IsChangePasswordOpen = false;
        CurrentPassword = "";
        NewPassword = "";
        ConfirmPassword = "";
        PasswordMessage = "";
        SelectedNav = NavItems[0];
        Status = "已退出，可重新登录。";
    }

    [RelayCommand]
    private void OpenChangePassword()
    {
        CurrentPassword = "";
        NewPassword = "";
        ConfirmPassword = "";
        PasswordMessage = "";
        PasswordSucceeded = false;
        IsChangePasswordOpen = true;
    }

    [RelayCommand]
    private void CloseChangePassword()
    {
        IsChangePasswordOpen = false;
        CurrentPassword = "";
        NewPassword = "";
        ConfirmPassword = "";
        PasswordMessage = "";
        PasswordSucceeded = false;
    }

    private bool CanChangePassword() =>
        !Busy.IsBusy &&
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    [RelayCommand(CanExecute = nameof(CanChangePassword))]
    private async Task ChangePasswordAsync()
    {
        PasswordSucceeded = false;
        if (NewPassword != ConfirmPassword)
        {
            PasswordMessage = "两次输入的新密码不一致。";
            ToastWarn("两次输入的新密码不一致。");
            return;
        }

        try
        {
            await Busy.RunAsync("正在修改密码…", () => _api.ChangePasswordAsync(CurrentPassword, NewPassword));
            PasswordSucceeded = true;
            PasswordMessage = "密码已更新。";
            ToastSuccess("密码已更新。");
            CurrentPassword = "";
            NewPassword = "";
            ConfirmPassword = "";
        }
        catch (Exception ex)
        {
            PasswordMessage = ex.Message;
            ToastError(ex.Message);
        }
    }

    private static string FriendlyLoginError(string raw)
    {
        if (raw.Contains("401", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            return "账号或密码不正确，请再试一次。";
        }

        if (raw.Contains("refused", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("failed to fetch", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
            raw.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
        {
            return "暂时连不上服务，请确认 API 已启动后再登录。";
        }

        return string.IsNullOrWhiteSpace(raw) ? "登录失败，请稍后再试。" : raw;
    }

    private void RefreshSiteName()
    {
        SiteName = string.IsNullOrWhiteSpace(About.Headline) ? "YJCabin" : About.Headline.Trim();
    }
}
