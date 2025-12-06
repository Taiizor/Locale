using Locale.Formats;
using Locale.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Locale.Services;

/// <summary>
/// Available validation rules.
/// </summary>
public static class CheckRules
{
    /// <summary>
    /// Rule that checks for empty or whitespace-only values in localization entries.
    /// </summary>
    public const string NoEmptyValues = "no-empty-values";

    /// <summary>
    /// Rule that checks for duplicate keys within the same localization file.
    /// </summary>
    public const string NoDuplicateKeys = "no-duplicate-keys";

    /// <summary>
    /// Rule that checks for keys that exist in target files but not in the base file.
    /// </summary>
    public const string NoOrphanKeys = "no-orphan-keys";

    /// <summary>
    /// Rule that checks for placeholder mismatches between base and target translations.
    /// </summary>
    public const string ConsistentPlaceholders = "consistent-placeholders";

    /// <summary>
    /// Rule that checks for trailing whitespace in localization values.
    /// </summary>
    public const string NoTrailingWhitespace = "no-trailing-whitespace";

    /// <summary>
    /// Array containing all available validation rule names.
    /// </summary>
    public static readonly string[] All =
    [
        NoEmptyValues,
        NoDuplicateKeys,
        NoOrphanKeys,
        ConsistentPlaceholders,
        NoTrailingWhitespace
    ];
}

/// <summary>
/// Options for the check operation.
/// </summary>
public sealed class CheckOptions
{
    /// <summary>
    /// Gets or sets the rules to check. If empty, all rules are checked.
    /// </summary>
    public List<string> Rules { get; set; } = [];

    /// <summary>
    /// Gets or sets the base culture for orphan key detection.
    /// </summary>
    public string? BaseCulture { get; set; }

    /// <summary>
    /// Gets or sets whether to scan directories recursively.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder pattern to use for matching.
    /// </summary>
    public string PlaceholderPattern { get; set; } = PlaceholderHelper.DefaultPlaceholderPattern;
}

/// <summary>
/// Service for validating localization files.
/// </summary>
public sealed class CheckService(FormatRegistry registry)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckService"/> class with the default format registry.
    /// </summary>
    public CheckService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Validates localization files in a path.
    /// </summary>
    public CheckReport Check(string path, CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        CheckReport report = new();

        List<string> rules = options.Rules.Count > 0 ? options.Rules : [.. CheckRules.All];
        List<LocalizationFile> files = DiscoverFiles(path, options.Recursive);

        foreach (LocalizationFile file in files)
        {
            CheckFile(file, rules, options, report);
        }

        // Cross-file checks
        if (rules.Contains(CheckRules.NoOrphanKeys) && !string.IsNullOrEmpty(options.BaseCulture))
        {
            CheckOrphanKeys(files, options.BaseCulture, report);
        }

        if (rules.Contains(CheckRules.ConsistentPlaceholders) && !string.IsNullOrEmpty(options.BaseCulture))
        {
            CheckPlaceholders(files, options.BaseCulture, options.PlaceholderPattern, report);
        }

        return report;
    }

    /// <summary>
    /// Validates a single localization file.
    /// </summary>
    public CheckReport Check(LocalizationFile file, CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        CheckReport report = new();
        List<string> rules = options.Rules.Count > 0 ? options.Rules : [.. CheckRules.All];

        CheckFile(file, rules, options, report);

        return report;
    }

    private void CheckFile(LocalizationFile file, List<string> rules, CheckOptions options, CheckReport report)
    {
        if (rules.Contains(CheckRules.NoEmptyValues))
        {
            CheckEmptyValues(file, report);
        }

        if (rules.Contains(CheckRules.NoDuplicateKeys))
        {
            CheckDuplicateKeys(file, report);
        }

        if (rules.Contains(CheckRules.NoTrailingWhitespace))
        {
            CheckTrailingWhitespace(file, report);
        }
    }

    private static void CheckEmptyValues(LocalizationFile file, CheckReport report)
    {
        foreach (LocalizationEntry entry in file.Entries)
        {
            if (entry.IsEmpty)
            {
                report.Violations.Add(new CheckViolation
                {
                    RuleName = CheckRules.NoEmptyValues,
                    FilePath = file.FilePath,
                    Key = entry.Key,
                    Message = $"Key '{entry.Key}' has an empty or whitespace-only value.",
                    Severity = ViolationSeverity.Warning
                });
            }
        }
    }

    private static void CheckDuplicateKeys(LocalizationFile file, CheckReport report)
    {
        HashSet<string> seenKeys = [];

        foreach (LocalizationEntry entry in file.Entries)
        {
            if (!seenKeys.Add(entry.Key))
            {
                report.Violations.Add(new CheckViolation
                {
                    RuleName = CheckRules.NoDuplicateKeys,
                    FilePath = file.FilePath,
                    Key = entry.Key,
                    Message = $"Duplicate key '{entry.Key}' found.",
                    Severity = ViolationSeverity.Error
                });
            }
        }
    }

    private static void CheckTrailingWhitespace(LocalizationFile file, CheckReport report)
    {
        foreach (LocalizationEntry entry in file.Entries)
        {
            if (entry.Value != null && entry.Value != entry.Value.TrimEnd())
            {
                report.Violations.Add(new CheckViolation
                {
                    RuleName = CheckRules.NoTrailingWhitespace,
                    FilePath = file.FilePath,
                    Key = entry.Key,
                    Message = $"Key '{entry.Key}' has trailing whitespace.",
                    Severity = ViolationSeverity.Warning
                });
            }
        }
    }

    private static void CheckOrphanKeys(List<LocalizationFile> files, string baseCulture, CheckReport report)
    {
        List<LocalizationFile> baseFiles = [.. files.Where(f =>
            f.Culture?.Equals(baseCulture, StringComparison.OrdinalIgnoreCase) == true)];

        HashSet<string> baseKeys = [.. baseFiles.SelectMany(f => f.Entries.Select(e => e.Key))];

        foreach (LocalizationFile file in files)
        {
            if (file.Culture?.Equals(baseCulture, StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            foreach (LocalizationEntry entry in file.Entries)
            {
                if (!baseKeys.Contains(entry.Key))
                {
                    report.Violations.Add(new CheckViolation
                    {
                        RuleName = CheckRules.NoOrphanKeys,
                        FilePath = file.FilePath,
                        Key = entry.Key,
                        Message = $"Key '{entry.Key}' exists in {file.Culture} but not in base culture {baseCulture}.",
                        Severity = ViolationSeverity.Warning
                    });
                }
            }
        }
    }

    private static void CheckPlaceholders(List<LocalizationFile> files, string baseCulture, string pattern, CheckReport report)
    {
        Regex regex = PlaceholderHelper.GetRegex(pattern);

        List<LocalizationFile> baseFiles = [.. files.Where(f =>
            f.Culture?.Equals(baseCulture, StringComparison.OrdinalIgnoreCase) == true)];

        Dictionary<string, List<string>> baseEntries = [];
        foreach (LocalizationFile file in baseFiles)
        {
            foreach (LocalizationEntry entry in file.Entries)
            {
                List<string> placeholders = PlaceholderHelper.ExtractPlaceholders(entry.Value, regex);
                baseEntries[entry.Key] = placeholders;
            }
        }

        foreach (LocalizationFile file in files)
        {
            if (file.Culture?.Equals(baseCulture, StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            foreach (LocalizationEntry entry in file.Entries)
            {
                if (!baseEntries.TryGetValue(entry.Key, out List<string>? basePlaceholders))
                {
                    continue;
                }

                List<string> targetPlaceholders = PlaceholderHelper.ExtractPlaceholders(entry.Value, regex);

                if (!basePlaceholders.SequenceEqual(targetPlaceholders))
                {
                    report.Violations.Add(new CheckViolation
                    {
                        RuleName = CheckRules.ConsistentPlaceholders,
                        FilePath = file.FilePath,
                        Key = entry.Key,
                        Message = $"Key '{entry.Key}' has different placeholders: base=[{string.Join(", ", basePlaceholders)}], target=[{string.Join(", ", targetPlaceholders)}].",
                        Severity = ViolationSeverity.Error
                    });
                }
            }
        }
    }

    private List<LocalizationFile> DiscoverFiles(string path, bool recursive)
    {
        List<LocalizationFile> files = [];
        SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<string> filePaths;

        if (File.Exists(path))
        {
            filePaths = [path];
        }
        else if (Directory.Exists(path))
        {
            filePaths = Directory.EnumerateFiles(path, "*.*", searchOption)
                .Where(registry.IsSupported);
        }
        else
        {
            return files;
        }

        foreach (string filePath in filePaths)
        {
            ILocalizationFormat? format = registry.GetFormatForFile(filePath);
            if (format == null)
            {
                continue;
            }

            try
            {
                files.Add(format.Parse(filePath));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                // Skip files that fail to parse due to IO or format issues
            }
        }

        return files;
    }
}