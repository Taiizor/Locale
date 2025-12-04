using System.Globalization;

namespace Locale.Models;

/// <summary>
/// Represents a localization file containing multiple entries for a specific culture.
/// </summary>
public sealed class LocalizationFile
{
    /// <summary>
    /// Gets or sets the file path (absolute or relative).
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the detected or configured culture (e.g., "en", "tr", "de").
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the format of this file (e.g., "json", "yaml", "resx").
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets the collection of localization entries in this file.
    /// </summary>
    public List<LocalizationEntry> Entries { get; init; } = [];

    /// <summary>
    /// Gets a dictionary mapping keys to entries for quick lookup.
    /// </summary>
    public IReadOnlyDictionary<string, LocalizationEntry> EntriesByKey =>
        Entries.ToDictionary(e => e.Key, e => e);

    /// <summary>
    /// Gets all keys in this file.
    /// </summary>
    public IEnumerable<string> Keys => Entries.Select(e => e.Key);

    /// <summary>
    /// Gets the number of entries in this file.
    /// </summary>
    public int Count => Entries.Count;

    /// <summary>
    /// Attempts to get the culture information from the Culture string.
    /// </summary>
    public CultureInfo? GetCultureInfo()
    {
        if (string.IsNullOrEmpty(Culture))
        {
            return null;
        }

        try
        {
            return CultureInfo.GetCultureInfo(Culture);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the value for a specific key, or null if not found.
    /// </summary>
    public string? GetValue(string key)
    {
        return Entries.FirstOrDefault(e => e.Key == key)?.Value;
    }

    /// <summary>
    /// Determines whether this file contains the specified key.
    /// </summary>
    public bool ContainsKey(string key)
    {
        return Entries.Any(e => e.Key == key);
    }

    /// <summary>
    /// Returns a string representation of this file including path, culture, and entry count.
    /// </summary>
    /// <returns>A string representation of the file.</returns>
    public override string ToString()
    {
        return $"{FilePath} ({Culture ?? "unknown"}) - {Count} entries";
    }
}