using CommunityToolkit.Mvvm.ComponentModel;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected static void ToastSuccess(string text) => ToastCenter.Current.Success(text);

    protected static void ToastInfo(string text) => ToastCenter.Current.Info(text);

    protected static void ToastWarn(string text) => ToastCenter.Current.Warning(text);

    protected static void ToastError(string text) => ToastCenter.Current.Error(text);
}
