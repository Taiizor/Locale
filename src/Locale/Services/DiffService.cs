using Locale.Formats;
using Locale.Models;
using System.Text.RegularExpressions;

namespace Locale.Services;

/// <summary>
/// Options for the diff operation.
/// </summary>
public sealed class DiffOptions
{
    /// <summary>
    /// Gets or sets whether to check for placeholder mismatches.
    /// </summary>
    public bool CheckPlaceholders { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder pattern to use for matching.
    /// </summary>
    public string PlaceholderPattern { get; set; } = @"\{+\w+\}+";
}

/// <summary>
/// Service for comparing two localization files.
/// </summary>
public sealed class DiffService(FormatRegistry registry)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffService"/> class with the default format registry.
    /// </summary>
    public DiffService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Compares two localization files.
    /// </summary>
    public DiffReport Diff(string firstPath, string secondPath, DiffOptions? options = null)
    {
        options ??= new DiffOptions();

        LocalizationFile firstFile = ParseFile(firstPath);
        LocalizationFile secondFile = ParseFile(secondPath);

        return Diff(firstFile, secondFile, options);
    }

    /// <summary>
    /// Compares two localization files.
    /// </summary>
    public DiffReport Diff(LocalizationFile first, LocalizationFile second, DiffOptions? options = null)
    {
        options ??= new DiffOptions();

        DiffReport report = new()
        {
            FirstFilePath = first.FilePath,
            SecondFilePath = second.FilePath
        };

        HashSet<string> firstKeys = [.. first.Entries.Select(e => e.Key)];
        Dictionary<string, LocalizationEntry> secondEntries = second.Entries.ToDictionary(e => e.Key, e => e);

        // Find keys only in first
        foreach (string key in firstKeys)
        {
            if (!secondEntries.ContainsKey(key))
            {
                report.OnlyInFirst.Add(key);
            }
        }

        // Find keys only in second and empty values
        foreach (KeyValuePair<string, LocalizationEntry> kvp in secondEntries)
        {
            if (!firstKeys.Contains(kvp.Key))
            {
                report.OnlyInSecond.Add(kvp.Key);
            }

            if (kvp.Value.IsEmpty)
            {
                report.EmptyInSecond.Add(kvp.Key);
            }
        }

        // Check placeholders
        if (options.CheckPlaceholders)
        {
            Regex placeholderRegex = new(options.PlaceholderPattern);
            Dictionary<string, LocalizationEntry> firstEntries = first.Entries.ToDictionary(e => e.Key, e => e);

            foreach (KeyValuePair<string, LocalizationEntry> kvp in secondEntries)
            {
                if (!firstEntries.TryGetValue(kvp.Key, out LocalizationEntry? firstEntry))
                {
                    continue;
                }

                List<string> firstPlaceholders = ExtractPlaceholders(firstEntry.Value, placeholderRegex);
                List<string> secondPlaceholders = ExtractPlaceholders(kvp.Value.Value, placeholderRegex);

                if (!firstPlaceholders.SequenceEqual(secondPlaceholders))
                {
                    report.PlaceholderMismatches.Add(new PlaceholderMismatch
                    {
                        Key = kvp.Key,
                        BasePlaceholders = firstPlaceholders,
                        TargetPlaceholders = secondPlaceholders
                    });
                }
            }
        }

        return report;
    }

    private LocalizationFile ParseFile(string filePath)
    {
        ILocalizationFormat format = registry.GetFormatForFile(filePath)
            ?? throw new NotSupportedException($"Unsupported file format: {filePath}");

        return format.Parse(filePath);
    }

    private static List<string> ExtractPlaceholders(string? value, Regex regex)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return [.. regex.Matches(value)
            .Select(m => m.Value)
            .OrderBy(p => p)];
    }
}