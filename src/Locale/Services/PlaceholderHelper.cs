using System.Text.RegularExpressions;

namespace Locale.Services;

/// <summary>
/// Helper class for placeholder extraction and comparison in localization values.
/// </summary>
internal static partial class PlaceholderHelper
{
    /// <summary>
    /// Default regex pattern for matching placeholders in localization values.
    /// Matches patterns like {name} and {{name}}.
    /// </summary>
    public const string DefaultPlaceholderPattern = @"\{+\w+\}+";

    /// <summary>
    /// Gets the default compiled placeholder regex.
    /// </summary>
    [GeneratedRegex(DefaultPlaceholderPattern)]
    public static partial Regex DefaultPlaceholderRegex();

    /// <summary>
    /// Extracts and sorts placeholders from a value string.
    /// </summary>
    /// <param name="value">The localization value to extract placeholders from.</param>
    /// <param name="regex">The regex pattern to use for extraction.</param>
    /// <returns>A sorted list of placeholder strings.</returns>
    public static List<string> ExtractPlaceholders(string? value, Regex regex)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return [.. regex.Matches(value)
            .Select(m => m.Value)
            .OrderBy(p => p)];
    }

    /// <summary>
    /// Extracts and sorts placeholders from a value string using the default pattern.
    /// </summary>
    /// <param name="value">The localization value to extract placeholders from.</param>
    /// <returns>A sorted list of placeholder strings.</returns>
    public static List<string> ExtractPlaceholders(string? value)
    {
        return ExtractPlaceholders(value, DefaultPlaceholderRegex());
    }

    /// <summary>
    /// Creates a regex from a pattern string, using the default pattern if null or empty.
    /// </summary>
    /// <param name="pattern">The regex pattern string, or null to use the default.</param>
    /// <returns>A compiled regex instance.</returns>
    public static Regex GetRegex(string? pattern)
    {
        if (string.IsNullOrEmpty(pattern) || pattern == DefaultPlaceholderPattern)
        {
            return DefaultPlaceholderRegex();
        }

        return new Regex(pattern, RegexOptions.Compiled);
    }
}