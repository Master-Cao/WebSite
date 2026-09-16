using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class MessagesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    [ObservableProperty] private ObservableCollection<ContactMessageDto> _items = [];
    [ObservableProperty] private ContactMessageDto? _selected;

    public MessagesViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    public async Task ReloadAsync()
    {
        var result = await _api.ListMessagesAsync();
        Items = new ObservableCollection<ContactMessageDto>(result.Items);
    }

    [RelayCommand]
    private async Task MarkReadAsync()
    {
        if (Selected is null) return;
        await _api.PatchMessageAsync(Selected.Id, true, null);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task MarkRepliedAsync()
    {
        if (Selected is null) return;
        await _api.PatchMessageAsync(Selected.Id, null, true);
        await ReloadAsync();
    }
}
