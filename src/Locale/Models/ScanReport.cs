namespace Locale.Models;

/// <summary>
/// Represents a scan report comparing a base culture with target cultures.
/// </summary>
public sealed class ScanReport
{
    /// <summary>
    /// Gets or sets the base culture that was used for comparison.
    /// </summary>
    public required string BaseCulture { get; set; }

    /// <summary>
    /// Gets or sets the target cultures that were compared.
    /// </summary>
    public List<string> TargetCultures { get; init; } = [];

    /// <summary>
    /// Gets the per-culture comparison results.
    /// </summary>
    public List<CultureComparisonResult> Results { get; init; } = [];

    /// <summary>
    /// Gets the total count of missing keys across all targets.
    /// </summary>
    public int TotalMissingKeys => Results.Sum(r => r.MissingKeys.Count);

    /// <summary>
    /// Gets the total count of orphan keys across all targets.
    /// </summary>
    public int TotalOrphanKeys => Results.Sum(r => r.OrphanKeys.Count);

    /// <summary>
    /// Gets the total count of empty values across all targets.
    /// </summary>
    public int TotalEmptyValues => Results.Sum(r => r.EmptyValues.Count);

    /// <summary>
    /// Gets whether the scan found any issues.
    /// </summary>
    public bool HasIssues => TotalMissingKeys > 0 || TotalOrphanKeys > 0 || TotalEmptyValues > 0;
}

/// <summary>
/// Represents the comparison result for a single target culture.
/// </summary>
public sealed class CultureComparisonResult
{
    /// <summary>
    /// Gets or sets the target culture.
    /// </summary>
    public required string Culture { get; set; }

    /// <summary>
    /// Gets or sets the file path for this culture.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets the keys that are in the base but missing from the target.
    /// </summary>
    public List<string> MissingKeys { get; init; } = [];

    /// <summary>
    /// Gets the keys that are in the target but not in the base (orphans).
    /// </summary>
    public List<string> OrphanKeys { get; init; } = [];

    /// <summary>
    /// Gets the keys that have empty or whitespace-only values in the target.
    /// </summary>
    public List<string> EmptyValues { get; init; } = [];

    /// <summary>
    /// Gets the keys that have placeholder mismatches between base and target.
    /// </summary>
    public List<PlaceholderMismatch> PlaceholderMismatches { get; init; } = [];

    /// <summary>
    /// Gets whether this culture has any issues.
    /// </summary>
    public bool HasIssues => MissingKeys.Count > 0 || OrphanKeys.Count > 0 ||
                             EmptyValues.Count > 0 || PlaceholderMismatches.Count > 0;
}

/// <summary>
/// Represents a placeholder mismatch between base and target values.
/// </summary>
public sealed class PlaceholderMismatch
{
    /// <summary>
    /// Gets or sets the key with the mismatch.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets the placeholders found in the base value.
    /// </summary>
    public List<string> BasePlaceholders { get; init; } = [];

    /// <summary>
    /// Gets the placeholders found in the target value.
    /// </summary>
    public List<string> TargetPlaceholders { get; init; } = [];
}