namespace Locale.Models;

/// <summary>
/// Represents the result of validation checks on localization files.
/// </summary>
public sealed class CheckReport
{
    /// <summary>
    /// Gets the list of violations found during validation.
    /// </summary>
    public List<CheckViolation> Violations { get; init; } = [];

    /// <summary>
    /// Gets whether any violations were found.
    /// </summary>
    public bool HasViolations => Violations.Count > 0;

    /// <summary>
    /// Gets the count of violations.
    /// </summary>
    public int ViolationCount => Violations.Count;

    /// <summary>
    /// Gets violations filtered by rule name.
    /// </summary>
    public IEnumerable<CheckViolation> GetViolationsByRule(string ruleName)
    {
        return Violations.Where(v => v.RuleName == ruleName);
    }

    /// <summary>
    /// Gets violations filtered by severity.
    /// </summary>
    public IEnumerable<CheckViolation> GetViolationsBySeverity(ViolationSeverity severity)
    {
        return Violations.Where(v => v.Severity == severity);
    }
}

/// <summary>
/// Represents a single validation violation.
/// </summary>
public sealed class CheckViolation
{
    /// <summary>
    /// Gets or sets the name of the rule that was violated.
    /// </summary>
    public required string RuleName { get; set; }

    /// <summary>
    /// Gets or sets the file path where the violation occurred.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the key involved in the violation (if applicable).
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message describing the violation.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the severity of the violation.
    /// </summary>
    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Warning;

    /// <summary>
    /// Returns a string representation of this violation including severity, rule, message, and file path.
    /// </summary>
    /// <returns>A string representation of the violation.</returns>
    public override string ToString()
    {
        return $"[{Severity}] {RuleName}: {Message} ({FilePath})";
    }
}

/// <summary>
/// Severity levels for validation violations.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning that should be reviewed.
    /// </summary>
    Warning,

    /// <summary>
    /// Error that should be fixed.
    /// </summary>
    Error
}