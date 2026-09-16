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
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException("slug", "Slug cannot be empty.");
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
        slug = EdgeDashes().Replace(slug, string.Empty);
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException("slug", "Slug cannot be empty after normalization.");
        }

        return slug;
    }

    public static bool IsValid(string slug) =>
        !string.IsNullOrWhiteSpace(slug) && slug == From(slug);
}
