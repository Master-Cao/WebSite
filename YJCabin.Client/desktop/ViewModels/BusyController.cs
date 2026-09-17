using CommunityToolkit.Mvvm.ComponentModel;

namespace YJCabin.Desktop.ViewModels;

public partial class BusyController : ViewModelBase
{
    private int _depth;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _message = "加载中…";

    public async Task RunAsync(string message, Func<Task> work)
    {
        Push(message);
        try
        {
            await work();
        }
        finally
        {
            Pop();
        }
    }

    public async Task<T> RunAsync<T>(string message, Func<Task<T>> work)
    {
        Push(message);
        try
        {
            return await work();
        }
        finally
        {
            Pop();
        }
    }

    private void Push(string message)
    {
        _depth++;
        Message = message;
        IsBusy = true;
    }

    private void Pop()
    {
        _depth = Math.Max(0, _depth - 1);
        if (_depth == 0)
        {
            IsBusy = false;
        }
    }
}
