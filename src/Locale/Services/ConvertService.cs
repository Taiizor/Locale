using Locale.Formats;
using Locale.Models;

namespace Locale.Services;

/// <summary>
/// Options for the convert operation.
/// </summary>
public sealed class ConvertOptions
{
    /// <summary>
    /// Gets or sets the source format (optional, auto-detected if not specified).
    /// </summary>
    public string? FromFormat { get; set; }

    /// <summary>
    /// Gets or sets the target format.
    /// </summary>
    public required string ToFormat { get; set; }

    /// <summary>
    /// Gets or sets whether to overwrite existing files.
    /// </summary>
    public bool Force { get; set; }

    /// <summary>
    /// Gets or sets whether to process directories recursively.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional culture override.
    /// </summary>
    public string? Culture { get; set; }
}

/// <summary>
/// Result of a conversion operation.
/// </summary>
public sealed class ConvertResult
{
    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    public required string SourcePath { get; set; }

    /// <summary>
    /// Gets or sets the destination file path.
    /// </summary>
    public required string DestinationPath { get; set; }

    /// <summary>
    /// Gets or sets whether the conversion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an error message if the conversion failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets any warnings generated during conversion.
    /// </summary>
    public List<string> Warnings { get; init; } = [];
}

/// <summary>
/// Service for converting between localization file formats.
/// </summary>
public sealed class ConvertService(FormatRegistry registry)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertService"/> class with the default format registry.
    /// </summary>
    public ConvertService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Converts a single file to a different format.
    /// </summary>
    public ConvertResult Convert(string sourcePath, string destinationPath, ConvertOptions options)
    {
        ConvertResult result = new()
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath
        };

        try
        {
            // Get source format
            ILocalizationFormat sourceFormat;
            if (!string.IsNullOrEmpty(options.FromFormat))
            {
                sourceFormat = registry.GetFormat(options.FromFormat)
                    ?? throw new NotSupportedException($"Unsupported source format: {options.FromFormat}");
            }
            else
            {
                sourceFormat = registry.GetFormatForFile(sourcePath)
                    ?? throw new NotSupportedException($"Cannot determine format for: {sourcePath}");
            }

            // Get target format
            ILocalizationFormat targetFormat = registry.GetFormat(options.ToFormat)
                ?? throw new NotSupportedException($"Unsupported target format: {options.ToFormat}");

            // Check if destination exists
            if (File.Exists(destinationPath) && !options.Force)
            {
                result.ErrorMessage = $"Destination file already exists: {destinationPath}. Use --force to overwrite.";
                return result;
            }

            // Parse source file
            LocalizationFile file = sourceFormat.Parse(sourcePath);

            // Override culture if specified
            if (!string.IsNullOrEmpty(options.Culture))
            {
                file.Culture = options.Culture;
            }

            // Update format
            file.Format = options.ToFormat;

            // Write to destination
            targetFormat.Write(file, destinationPath);

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Converts all files in a directory to a different format.
    /// </summary>
    public List<ConvertResult> ConvertDirectory(string sourcePath, string destinationPath, ConvertOptions options)
    {
        List<ConvertResult> results = [];

        if (!Directory.Exists(sourcePath))
        {
            results.Add(new ConvertResult
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath,
                ErrorMessage = $"Source directory does not exist: {sourcePath}"
            });
            return results;
        }

        SearchOption searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<string> sourceFiles = Directory.EnumerateFiles(sourcePath, "*.*", searchOption)
            .Where(registry.IsSupported);

        foreach (string sourceFile in sourceFiles)
        {
            // Calculate relative path
            string relativePath = Path.GetRelativePath(sourcePath, sourceFile);

            // Change extension
            string targetExtension = GetExtensionForFormat(options.ToFormat);
            string targetFileName = Path.ChangeExtension(relativePath, targetExtension);
            string targetPath = Path.Combine(destinationPath, targetFileName);

            ConvertResult result = Convert(sourceFile, targetPath, options);
            results.Add(result);
        }

        return results;
    }

    private string GetExtensionForFormat(string format)
    {
        ILocalizationFormat? formatHandler = registry.GetFormat(format);
        return formatHandler?.SupportedExtensions.FirstOrDefault() ?? $".{format}";
    }
}