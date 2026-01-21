namespace CleanAPIDemo.Common.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if the string is null, empty, or contains only whitespace.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Checks if the string has a meaningful value (not null, empty, or whitespace).
    /// </summary>
    public static bool HasValue(this string? value)
        => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Truncates a string to the specified maximum length, optionally adding an ellipsis.
    /// </summary>
    public static string Truncate(this string? value, int maxLength, bool addEllipsis = false)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        if (addEllipsis && maxLength > 3)
            return string.Concat(value.AsSpan(0, maxLength - 3), "...");

        return value[..maxLength];
    }

    /// <summary>
    /// Converts a string to title case (first letter of each word capitalized).
    /// </summary>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// Converts a string to a URL-friendly slug format.
    /// </summary>
    public static string ToSlug(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var slug = value.ToLowerInvariant().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    /// <summary>
    /// Returns a default value if the string is null or whitespace.
    /// </summary>
    public static string OrDefault(this string? value, string defaultValue)
        => string.IsNullOrWhiteSpace(value) ? defaultValue : value;

    /// <summary>
    /// Removes all whitespace from the string.
    /// </summary>
    public static string RemoveWhitespace(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
    }
}
