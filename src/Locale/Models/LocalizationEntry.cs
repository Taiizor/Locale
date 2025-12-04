namespace Locale.Models;

/// <summary>
/// Represents a single key-value localization entry.
/// </summary>
public sealed class LocalizationEntry
{
    /// <summary>
    /// Gets or sets the key (identifier) for this entry.
    /// For nested structures, this represents the flattened path (e.g., "Home.Title").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the translated value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets optional comments or metadata associated with this entry.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the source text (for formats like XLIFF that track source vs target).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Determines whether the value is empty or contains only whitespace.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

    /// <summary>
    /// Returns a string representation of this entry in the format "Key = Value".
    /// </summary>
    /// <returns>A string representation of the entry.</returns>
    public override string ToString()
    {
        return $"{Key} = {Value}";
    }

    /// <summary>
    /// Determines whether the specified object is equal to this entry.
    /// Two entries are considered equal if they have the same key and value.
    /// </summary>
    /// <param name="obj">The object to compare with this entry.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is LocalizationEntry other && Key == other.Key && Value == other.Value;
    }

    /// <summary>
    /// Returns a hash code for this entry based on the key and value.
    /// </summary>
    /// <returns>A hash code for the current entry.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Key, Value);
    }
}