# ADR-0001: Multi-Format Architecture

## Status

Accepted

Date: 2024-12-06

## Context

The Locale project needs to support multiple localization file formats (JSON, YAML, RESX, PO, XLIFF, SRT, VTT, CSV, i18next, Fluent FTL, VB) to serve different ecosystems and use cases. Each format has its own syntax, structure, and semantics.

**Requirements:**
- Support 11+ different localization formats
- Allow seamless conversion between formats
- Enable adding new formats without modifying core logic
- Maintain consistent behavior across all formats
- Support both reading and writing (except VB resources)

**Constraints:**
- .NET ecosystem (targeting .NET 8, 9, 10)
- Must be performant for large files (10,000+ entries)
- Must preserve data integrity during conversions
- Limited dependencies (prefer built-in .NET libraries)

## Decision

Implement a **plugin-based architecture** with:

1. **Common Data Model**: All formats parse to/from a unified `LocalizationFile` model containing `LocalizationEntry` objects
2. **Format Interface**: `ILocalizationFormat` interface that all format handlers implement
3. **Format Registry**: Central registry (`FormatRegistry`) for discovering and using format handlers
4. **Base Implementation**: `LocalizationFormatBase` abstract class providing common functionality

**Key Design Principles:**
- **Separation of Concerns**: Core models separate from format-specific parsing
- **Open/Closed Principle**: Open for extension (new formats) but closed for modification (core logic)
- **Single Responsibility**: Each format handler only handles its specific format
- **Dependency Inversion**: Core depends on abstractions, not concrete format implementations

**Implementation:**
```csharp
// Common interface
public interface ILocalizationFormat
{
    string FormatId { get; }
    IReadOnlyList<string> SupportedExtensions { get; }
    bool CanHandle(string filePath);
    LocalizationFile Parse(Stream stream, string? filePath = null);
    void Write(LocalizationFile file, Stream stream);
}

// Common data model
public sealed class LocalizationFile
{
    public required string FilePath { get; set; }
    public string? Culture { get; set; }
    public List<LocalizationEntry> Entries { get; init; } = [];
}

// Format registry
public sealed class FormatRegistry
{
    public static FormatRegistry Default { get; }
    public void Register(ILocalizationFormat format);
    public ILocalizationFormat? GetFormatForFile(string filePath);
}
```

## Consequences

### Positive

- **Extensibility**: New formats can be added by implementing `ILocalizationFormat` without modifying existing code
- **Testability**: Each format handler can be tested independently
- **Consistency**: All formats work through the same interface, ensuring consistent behavior
- **Conversion Flexibility**: Any format can convert to any other format through the common model
- **Clean Architecture**: Clear separation between core logic and format-specific implementations
- **Maintainability**: Changes to one format don't affect others

### Negative

- **Data Loss**: Some format-specific features may be lost during conversion (e.g., Fluent attributes, XLIFF translation units)
- **Flattening**: Nested structures (JSON/YAML) must be flattened to key-value pairs
- **Memory Usage**: All entries loaded into memory (not streaming)
- **Abstraction Overhead**: Small performance cost for interface dispatch
- **Learning Curve**: Contributors need to understand the architecture

### Neutral

- **Common Model**: Trade-off between format-specific richness and universal compatibility
- **Caching**: Format detection and key lookups are cached for performance
- **Read-Only Formats**: Some formats (VB resources) are read-only by design

## Alternatives Considered

### Alternative 1: Format-Specific APIs

**Description:** Separate API for each format with no common abstraction

**Pros:**
- Maximum flexibility per format
- No data loss during parsing
- Format-specific optimizations possible
- No abstraction overhead

**Cons:**
- No format conversion capability
- Code duplication across formats
- Inconsistent APIs
- Harder to test and maintain
- No unified tooling possible

**Why not chosen:** Defeats the purpose of a multi-format tool and prevents conversions

### Alternative 2: Direct Conversion Matrix

**Description:** Implement N×N conversion functions for all format pairs

**Pros:**
- Optimized conversions per format pair
- No intermediate representation needed
- Can preserve format-specific features

**Cons:**
- Exponential growth: 11 formats = 110 conversion functions
- Massive maintenance burden
- Code duplication
- Inconsistent conversion logic
- Adding a format requires N new converters

**Why not chosen:** Not scalable as formats increase

### Alternative 3: AST-Based Model

**Description:** Use an Abstract Syntax Tree representation that captures all format features

**Pros:**
- Can represent complex nested structures
- No data loss
- Format-specific features preserved
- Precise conversions

**Cons:**
- Much more complex implementation
- Harder to understand and maintain
- Performance overhead
- Overkill for most use cases
- Difficult to extend

**Why not chosen:** Too complex for the primary use case of simple key-value translations

### Alternative 4: Streaming Architecture

**Description:** Stream-based processing without loading everything into memory

**Pros:**
- Lower memory usage
- Can handle very large files
- Better performance for simple operations

**Cons:**
- Can't support random access (e.g., lookups)
- Harder to implement diff/scan operations
- More complex API
- Not all formats support streaming
- Most localization files are small enough

**Why not chosen:** Most localization files are small (<10,000 entries) and benefit from in-memory operations

## References

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Open/Closed Principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)
- [Plugin Architecture Pattern](https://en.wikipedia.org/wiki/Plug-in_(computing))
- Related ADR: [0002-format-registry-pattern.md](./0002-format-registry-pattern.md)