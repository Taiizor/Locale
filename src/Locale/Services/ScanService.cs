using Locale.Formats;
using Locale.Models;
using System.Text.RegularExpressions;

namespace Locale.Services;

/// <summary>
/// Options for the scan operation.
/// </summary>
public sealed class ScanOptions
{
    /// <summary>
    /// Gets or sets the base culture to compare against.
    /// </summary>
    public required string BaseCulture { get; set; }

    /// <summary>
    /// Gets or sets the target cultures to compare.
    /// </summary>
    public List<string> TargetCultures { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to scan directories recursively.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets glob patterns to ignore.
    /// </summary>
    public List<string> IgnorePatterns { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to check for placeholder mismatches.
    /// </summary>
    public bool CheckPlaceholders { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder pattern to use for matching.
    /// Default matches {name} and {{name}} patterns.
    /// </summary>
    public string PlaceholderPattern { get; set; } = @"\{+\w+\}+";
}

/// <summary>
/// Service for scanning and comparing localization files.
/// </summary>
public sealed class ScanService(FormatRegistry registry)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanService"/> class with the default format registry.
    /// </summary>
    public ScanService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Scans a directory for localization files and compares base to target cultures.
    /// </summary>
    public ScanReport Scan(string path, ScanOptions options)
    {
        IEnumerable<LocalizationFile> files = DiscoverFiles(path, options);
        Dictionary<string, List<LocalizationFile>> filesByCulture = GroupFilesByCulture(files);

        if (!filesByCulture.TryGetValue(options.BaseCulture.ToLowerInvariant(), out List<LocalizationFile>? baseFiles))
        {
            return new ScanReport
            {
                BaseCulture = options.BaseCulture,
                TargetCultures = options.TargetCultures
            };
        }

        // Merge all base files into one combined set of entries
        Dictionary<string, LocalizationEntry> baseEntries = [];
        foreach (LocalizationFile baseFile in baseFiles)
        {
            foreach (LocalizationEntry entry in baseFile.Entries)
            {
                baseEntries[entry.Key] = entry;
            }
        }

        ScanReport report = new()
        {
            BaseCulture = options.BaseCulture,
            TargetCultures = options.TargetCultures
        };

        List<string> targetCultures = options.TargetCultures.Count > 0
            ? options.TargetCultures
            : [.. filesByCulture.Keys.Where(c => !c.Equals(options.BaseCulture, StringComparison.OrdinalIgnoreCase))];

        foreach (string targetCulture in targetCultures)
        {
            CultureComparisonResult result = CompareWithBase(
                baseEntries,
                filesByCulture.GetValueOrDefault(targetCulture.ToLowerInvariant()) ?? [],
                targetCulture,
                options);

            report.Results.Add(result);
        }

        return report;
    }

    /// <summary>
    /// Discovers all supported localization files in a path.
    /// </summary>
    public IEnumerable<LocalizationFile> DiscoverFiles(string path, ScanOptions options)
    {
        SearchOption searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<string> files;

        if (File.Exists(path))
        {
            files = [path];
        }
        else if (Directory.Exists(path))
        {
            files = Directory.EnumerateFiles(path, "*.*", searchOption)
                .Where(registry.IsSupported);
        }
        else
        {
            yield break;
        }

        foreach (string filePath in files)
        {
            if (ShouldIgnore(filePath, options.IgnorePatterns))
            {
                continue;
            }

            ILocalizationFormat? format = registry.GetFormatForFile(filePath);
            if (format == null)
            {
                continue;
            }

            LocalizationFile? file = null;
            try
            {
                file = format.Parse(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            {
                // Skip files that fail to parse due to IO or format issues
                continue;
            }

            if (file != null)
            {
                yield return file;
            }
        }
    }

    private Dictionary<string, List<LocalizationFile>> GroupFilesByCulture(IEnumerable<LocalizationFile> files)
    {
        Dictionary<string, List<LocalizationFile>> result = new(StringComparer.OrdinalIgnoreCase);

        foreach (LocalizationFile file in files)
        {
            string? culture = file.Culture?.ToLowerInvariant();
            if (string.IsNullOrEmpty(culture))
            {
                continue;
            }

            if (!result.TryGetValue(culture, out List<LocalizationFile>? list))
            {
                list = [];
                result[culture] = list;
            }

            list.Add(file);
        }

        return result;
    }

    private CultureComparisonResult CompareWithBase(
        Dictionary<string, LocalizationEntry> baseEntries,
        List<LocalizationFile> targetFiles,
        string culture,
        ScanOptions options)
    {
        CultureComparisonResult result = new()
        {
            Culture = culture,
            FilePath = targetFiles.FirstOrDefault()?.FilePath
        };

        // Merge all target files
        Dictionary<string, LocalizationEntry> targetEntries = [];
        foreach (LocalizationFile file in targetFiles)
        {
            foreach (LocalizationEntry entry in file.Entries)
            {
                targetEntries[entry.Key] = entry;
            }
        }

        // Find missing keys
        foreach (string baseKey in baseEntries.Keys)
        {
            if (!targetEntries.ContainsKey(baseKey))
            {
                result.MissingKeys.Add(baseKey);
            }
        }

        // Find orphan keys and empty values
        foreach (KeyValuePair<string, LocalizationEntry> kvp in targetEntries)
        {
            if (!baseEntries.ContainsKey(kvp.Key))
            {
                result.OrphanKeys.Add(kvp.Key);
            }

            if (kvp.Value.IsEmpty)
            {
                result.EmptyValues.Add(kvp.Key);
            }
        }

        // Check placeholders
        if (options.CheckPlaceholders)
        {
            Regex placeholderRegex = new(options.PlaceholderPattern);

            foreach (KeyValuePair<string, LocalizationEntry> kvp in targetEntries)
            {
                if (!baseEntries.TryGetValue(kvp.Key, out LocalizationEntry? baseEntry))
                {
                    continue;
                }

                List<string> basePlaceholders = ExtractPlaceholders(baseEntry.Value, placeholderRegex);
                List<string> targetPlaceholders = ExtractPlaceholders(kvp.Value.Value, placeholderRegex);

                if (!basePlaceholders.SequenceEqual(targetPlaceholders))
                {
                    result.PlaceholderMismatches.Add(new PlaceholderMismatch
                    {
                        Key = kvp.Key,
                        BasePlaceholders = basePlaceholders,
                        TargetPlaceholders = targetPlaceholders
                    });
                }
            }
        }

        return result;
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

    private static bool ShouldIgnore(string filePath, List<string> patterns)
    {
        if (patterns.Count == 0)
        {
            return false;
        }

        string fileName = Path.GetFileName(filePath);

        foreach (string pattern in patterns)
        {
            if (fileName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}