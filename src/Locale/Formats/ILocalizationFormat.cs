using Locale.Models;

namespace Locale.Formats;

/// <summary>
/// Interface for localization file format handlers.
/// </summary>
public interface ILocalizationFormat
{
    /// <summary>
    /// Gets the format identifier (e.g., "json", "yaml", "resx").
    /// </summary>
    string FormatId { get; }

    /// <summary>
    /// Gets the file extensions supported by this format.
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>
    /// Determines whether this format can handle the specified file.
    /// </summary>
    bool CanHandle(string filePath);

    /// <summary>
    /// Parses a localization file and returns a LocalizationFile instance.
    /// </summary>
    LocalizationFile Parse(string filePath);

    /// <summary>
    /// Parses localization content from a stream.
    /// </summary>
    LocalizationFile Parse(Stream stream, string? filePath = null);

    /// <summary>
    /// Writes a LocalizationFile to a file.
    /// </summary>
    void Write(LocalizationFile file, string filePath);

    /// <summary>
    /// Writes a LocalizationFile to a stream.
    /// </summary>
    void Write(LocalizationFile file, Stream stream);
}