using System.Text;
using System.Text.RegularExpressions;

namespace YJCabin.Application.Common;

public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonSlugChars();

    [GeneratedRegex(@"^-+|-+$")]
    private static partial Regex EdgeDashes();

    public static string From(string value)
    {
        var slug = Normalize(value);
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("slug", "Slug cannot be empty.");
        }

        return slug;
    }

    public static string FromTitle(string title) => FromName(title, "article");

    public static string FromName(string value, string fallbackPrefix)
    {
        var slug = Normalize(value);
        return string.IsNullOrWhiteSpace(slug)
            ? $"{fallbackPrefix}-{Guid.NewGuid().ToString("N")[..8]}"
            : slug;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = char.GetUnicodeCategory(ch);
            if (category == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(ch);
        }

        var slug = NonSlugChars().Replace(builder.ToString(), "-");
        return EdgeDashes().Replace(slug, string.Empty);
    }

    public static bool IsUsable(string? slug) =>
        !string.IsNullOrWhiteSpace(slug) &&
        !slug.Equals("undefined", StringComparison.OrdinalIgnoreCase) &&
        !slug.Equals("null", StringComparison.OrdinalIgnoreCase);

    public static bool IsValid(string slug) =>
        IsUsable(slug) && slug == From(slug);
}
