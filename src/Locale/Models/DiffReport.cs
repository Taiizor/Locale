namespace Locale.Models;

/// <summary>
/// Represents a diff report comparing two localization files.
/// </summary>
public sealed class DiffReport
{
    /// <summary>
    /// Gets or sets the first file path.
    /// </summary>
    public required string FirstFilePath { get; set; }

    /// <summary>
    /// Gets or sets the second file path.
    /// </summary>
    public required string SecondFilePath { get; set; }

    /// <summary>
    /// Gets the keys that are only in the first file.
    /// </summary>
    public List<string> OnlyInFirst { get; init; } = [];

    /// <summary>
    /// Gets the keys that are only in the second file.
    /// </summary>
    public List<string> OnlyInSecond { get; init; } = [];

    /// <summary>
    /// Gets the keys that have empty values in the second file.
    /// </summary>
    public List<string> EmptyInSecond { get; init; } = [];

    /// <summary>
    /// Gets the placeholder mismatches between the two files.
    /// </summary>
    public List<PlaceholderMismatch> PlaceholderMismatches { get; init; } = [];

    /// <summary>
    /// Gets whether the diff found any differences.
    /// </summary>
    public bool HasDifferences => OnlyInFirst.Count > 0 || OnlyInSecond.Count > 0 ||
                                  EmptyInSecond.Count > 0 || PlaceholderMismatches.Count > 0;
}