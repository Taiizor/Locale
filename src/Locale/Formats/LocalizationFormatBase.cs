using Locale.Models;

namespace Locale.Formats;

/// <summary>
/// Abstract base class for localization format handlers.
/// </summary>
public abstract class LocalizationFormatBase : ILocalizationFormat
{
    /// <inheritdoc />
    public abstract string FormatId { get; }

    /// <inheritdoc />
    public abstract IReadOnlyList<string> SupportedExtensions { get; }

    /// <inheritdoc />
    public virtual bool CanHandle(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    /// <inheritdoc />
    public virtual LocalizationFile Parse(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return Parse(stream, filePath);
    }

    /// <inheritdoc />
    public abstract LocalizationFile Parse(Stream stream, string? filePath = null);

    /// <inheritdoc />
    public virtual void Write(LocalizationFile file, string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(filePath);
        Write(file, stream);
    }

    /// <inheritdoc />
    public abstract void Write(LocalizationFile file, Stream stream);

    /// <summary>
    /// Detects the culture from a file name using common patterns.
    /// </summary>
    protected static string? DetectCultureFromFileName(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Try patterns like: en.json, common.tr.resx, Home.de.yaml
        string[] parts = fileName.Split('.');

        // Check if the last part looks like a culture code
        if (parts.Length >= 1)
        {
            string lastPart = parts[^1];
            if (LooksLikeCultureCode(lastPart))
            {
                return lastPart.ToLowerInvariant();
            }
        }

        // Check if the filename is just a culture code (e.g., "en")
        if (parts.Length == 1 && LooksLikeCultureCode(parts[0]))
        {
            return parts[0].ToLowerInvariant();
        }

        return null;
    }

    /// <summary>
    /// Checks if a string looks like a culture code (e.g., "en", "en-US", "tr").
    /// </summary>
    private static bool LooksLikeCultureCode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Simple culture codes: en, tr, de, etc.
        if (value.Length == 2 && value.All(char.IsLetter))
        {
            return true;
        }

        // Full culture codes: en-US, tr-TR, de-DE, etc.
        if (value.Length >= 4 && value.Length <= 5 && value.Contains('-'))
        {
            string[] cultureParts = value.Split('-');
            return cultureParts.Length == 2 &&
                   cultureParts[0].Length == 2 &&
                   cultureParts[0].All(char.IsLetter) &&
                   cultureParts[1].All(char.IsLetterOrDigit);
        }

        return false;
    }
}