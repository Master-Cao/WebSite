namespace YJCabin.Application.Common;

public sealed class ListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public string? Tag { get; init; }
    public string? Q { get; init; }

    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize is < 1 or > 50 ? 12 : PageSize;
}
