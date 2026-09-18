using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class MessagesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly BusyController _busy;

    [ObservableProperty] private ObservableCollection<ContactMessageDto> _items = [];
    [ObservableProperty] private ContactMessageDto? _selected;
    [ObservableProperty] private string _message = "";

    public bool HasItems => Items.Count > 0;
    public bool ShowDetail => Selected is not null;

    public MessagesViewModel(ApiClient api, BusyController busy)
    {
        _api = api;
        _busy = busy;
    }

    partial void OnItemsChanged(ObservableCollection<ContactMessageDto> value) =>
        OnPropertyChanged(nameof(HasItems));

    partial void OnSelectedChanged(ContactMessageDto? value)
    {
        OnPropertyChanged(nameof(ShowDetail));
        Message = value is null && Items.Count > 0 ? "从左侧选择一条留言。" : "";
    }

    [RelayCommand]
    public Task ReloadAsync() => _busy.RunAsync("正在加载留言…", LoadListAsync);

    private async Task LoadListAsync()
    {
        var id = Selected?.Id;
        var result = await _api.ListMessagesAsync();
        Items = new ObservableCollection<ContactMessageDto>(result.Items);
        Selected = id is null ? null : Items.FirstOrDefault(item => item.Id == id);
        if (Selected is null)
        {
            Message = Items.Count == 0 ? "暂无留言。" : "从左侧选择一条留言。";
        }
    }

    [RelayCommand]
    private Task MarkReadAsync() => _busy.RunAsync("正在更新留言…", async () =>
    {
        if (Selected is null)
        {
            return;
        }

        try
        {
            await _api.PatchMessageAsync(Selected.Id, true, null);
            await LoadListAsync();
            ToastSuccess("已标为已读。");
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    });

    [RelayCommand]
    private Task MarkRepliedAsync() => _busy.RunAsync("正在更新留言…", async () =>
    {
        if (Selected is null)
        {
            return;
        }

        try
        {
            await _api.PatchMessageAsync(Selected.Id, null, true);
            await LoadListAsync();
            ToastSuccess("已标为已回复。");
        }
        catch (Exception ex)
        {
            ToastError(ex.Message);
        }
    });

    [RelayCommand]
    private void OpenItem(ContactMessageDto? item)
    {
        if (item is null)
        {
            return;
        }

        Selected = item;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        Selected = null;
        Message = HasItems ? "选择一张卡片查看详情。" : "暂无留言。";
    }
}
