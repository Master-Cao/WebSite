using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using YJCabin.Desktop.Services;

namespace YJCabin.Desktop.ViewModels;

public partial class TagChoice : ObservableObject
{
    public TagChoice(string name, bool isSelected)
    {
        Name = name;
        IsSelected = isSelected;
    }

    public string Name { get; }

    [ObservableProperty] private bool _isSelected;
}

internal static class TagCatalog
{
    public static async Task<ObservableCollection<TagChoice>> LoadAsync(
        ApiClient api,
        IEnumerable<string> selected,
        CancellationToken cancellationToken = default)
    {
        var names = selected
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();
        var catalog = await api.ListTagsAsync(cancellationToken);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new ObservableCollection<TagChoice>();
        foreach (var tag in catalog)
        {
            if (!seen.Add(tag.Name))
            {
                continue;
            }

            items.Add(new TagChoice(tag.Name, names.Exists(name => name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase))));
        }

        foreach (var name in names.Where(seen.Add))
        {
            items.Add(new TagChoice(name, true));
        }

        return items;
    }

    public static List<string> SelectedNames(IEnumerable<TagChoice> choices) =>
        choices.Where(x => x.IsSelected).Select(x => x.Name).ToList();

    public static void Add(ObservableCollection<TagChoice> choices, string raw)
    {
        var name = raw.Trim();
        if (name.Length == 0)
        {
            return;
        }

        var existing = choices.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.IsSelected = true;
            return;
        }

        choices.Add(new TagChoice(name, true));
    }
}

