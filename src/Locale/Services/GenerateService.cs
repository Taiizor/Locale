using Locale.Formats;
using Locale.Models;

namespace Locale.Services;

/// <summary>
/// Options for the generate operation.
/// </summary>
public sealed class GenerateOptions
{
    /// <summary>
    /// Gets or sets the target culture to generate files for.
    /// </summary>
    public required string TargetCulture { get; set; }

    /// <summary>
    /// Gets or sets the base culture to generate from.
    /// </summary>
    public required string BaseCulture { get; set; }

    /// <summary>
    /// Gets or sets the placeholder value for missing translations.
    /// </summary>
    public string PlaceholderPattern { get; set; } = "@@MISSING@@ {0}";

    /// <summary>
    /// Gets or sets whether to use empty strings instead of placeholders.
    /// </summary>
    public bool UseEmptyValue { get; set; }

    /// <summary>
    /// Gets or sets whether to overwrite existing keys.
    /// </summary>
    public bool OverwriteExisting { get; set; }

    /// <summary>
    /// Gets or sets whether to process directories recursively.
    /// </summary>
    public bool Recursive { get; set; } = true;
}

/// <summary>
/// Result of a generate operation.
/// </summary>
public sealed class GenerateResult
{
    /// <summary>
    /// Gets or sets the generated file path.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets whether the file was created (vs updated).
    /// </summary>
    public bool Created { get; set; }

    /// <summary>
    /// Gets or sets the number of keys added.
    /// </summary>
    public int KeysAdded { get; set; }

    /// <summary>
    /// Gets or sets the number of keys skipped (already existed).
    /// </summary>
    public int KeysSkipped { get; set; }

    /// <summary>
    /// Gets or sets any error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets whether the operation was successful.
    /// </summary>
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
}

/// <summary>
/// Service for generating skeleton localization files.
/// </summary>
public sealed class GenerateService(FormatRegistry registry)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateService"/> class with the default format registry.
    /// </summary>
    public GenerateService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Generates skeleton target files from base culture files.
    /// </summary>
    public List<GenerateResult> Generate(string inputPath, string outputPath, GenerateOptions options)
    {
        List<GenerateResult> results = [];
        SearchOption searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<string> baseFiles;

        if (File.Exists(inputPath))
        {
            baseFiles = [inputPath];
        }
        else if (Directory.Exists(inputPath))
        {
            baseFiles = Directory.EnumerateFiles(inputPath, "*.*", searchOption)
                .Where(registry.IsSupported);
        }
        else
        {
            results.Add(new GenerateResult
            {
                FilePath = inputPath,
                ErrorMessage = $"Input path does not exist: {inputPath}"
            });
            return results;
        }

        foreach (string baseFilePath in baseFiles)
        {
            ILocalizationFormat? format = registry.GetFormatForFile(baseFilePath);
            if (format == null)
            {
                continue;
            }

            // Check if this is a base culture file
            LocalizationFile baseFile;
            try
            {
                baseFile = format.Parse(baseFilePath);
            }
            catch (Exception ex)
            {
                results.Add(new GenerateResult
                {
                    FilePath = baseFilePath,
                    ErrorMessage = $"Failed to parse: {ex.Message}"
                });
                continue;
            }

            if (baseFile.Culture?.Equals(options.BaseCulture, StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            // Generate target file path
            string targetFilePath = GenerateTargetPath(baseFilePath, inputPath, outputPath,
                options.BaseCulture, options.TargetCulture);

            GenerateResult result = GenerateTargetFile(baseFile, targetFilePath, format, options);
            results.Add(result);
        }

        return results;
    }

    private GenerateResult GenerateTargetFile(LocalizationFile baseFile, string targetFilePath,
        ILocalizationFormat format, GenerateOptions options)
    {
        GenerateResult result = new() { FilePath = targetFilePath };

        try
        {
            List<LocalizationEntry> targetEntries = [];
            Dictionary<string, LocalizationEntry> existingEntries = [];

            // Load existing file if it exists
            if (File.Exists(targetFilePath))
            {
                result.Created = false;
                try
                {
                    LocalizationFile existingFile = format.Parse(targetFilePath);
                    foreach (LocalizationEntry entry in existingFile.Entries)
                    {
                        existingEntries[entry.Key] = entry;
                    }
                }
                catch
                {
                    // Ignore parse errors for existing file
                }
            }
            else
            {
                result.Created = true;
            }

            // Generate entries from base file
            foreach (LocalizationEntry baseEntry in baseFile.Entries)
            {
                if (existingEntries.TryGetValue(baseEntry.Key, out LocalizationEntry? existingEntry))
                {
                    if (options.OverwriteExisting)
                    {
                        targetEntries.Add(CreatePlaceholderEntry(baseEntry, options));
                        result.KeysAdded++;
                    }
                    else
                    {
                        targetEntries.Add(existingEntry);
                        result.KeysSkipped++;
                    }
                }
                else
                {
                    targetEntries.Add(CreatePlaceholderEntry(baseEntry, options));
                    result.KeysAdded++;
                }
            }

            // Create target file
            LocalizationFile targetFile = new()
            {
                FilePath = targetFilePath,
                Culture = options.TargetCulture,
                Format = format.FormatId,
                Entries = targetEntries
            };

            format.Write(targetFile, targetFilePath);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static LocalizationEntry CreatePlaceholderEntry(LocalizationEntry baseEntry, GenerateOptions options)
    {
        string value;
        if (options.UseEmptyValue)
        {
            value = "";
        }
        else
        {
            value = string.Format(options.PlaceholderPattern, baseEntry.Value ?? "");
        }

        return new LocalizationEntry
        {
            Key = baseEntry.Key,
            Value = value,
            Comment = baseEntry.Comment,
            Source = baseEntry.Value
        };
    }

    private static string GenerateTargetPath(string baseFilePath, string inputPath, string outputPath,
        string baseCulture, string targetCulture)
    {
        return PathHelper.GenerateTargetPath(baseFilePath, inputPath, outputPath, baseCulture, targetCulture);
    }
}