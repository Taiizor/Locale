# Locale Documentation

Welcome to the Locale documentation! This directory contains comprehensive guides for developers and contributors.

## 📚 Documentation Index

### For Users

- **[Main README](../README.md)** - Project overview, installation, and quick start
- **[API Reference](./API-REFERENCE.md)** - Complete API documentation for the library
- **[FAQ](./FAQ.md)** - Frequently asked questions and answers
- **[Troubleshooting Guide](./TROUBLESHOOTING.md)** - Common issues and solutions
- **[Format Comparison](./FORMAT-COMPARISON.md)** - Detailed comparison of all supported formats
- **[Performance Guide](./PERFORMANCE.md)** - Performance optimization best practices
- **[Examples](../examples/)** - Practical examples and real-world usage scenarios
  - [Basic Usage](../examples/basic-usage/)
  - [Format Conversion](../examples/format-conversion/)
  - [CI/CD Integration](../examples/ci-integration/)
  - [Custom Formats](../examples/custom-format/)
- **[CHANGELOG](../CHANGELOG.md)** - Version history and release notes

### For Contributors

- **[Contributing Guide](../.github/CONTRIBUTING.md)** - How to contribute to the project
- **[Error Handling Guidelines](./ERROR-HANDLING.md)** - Best practices for error handling
- **[Code of Conduct](../.github/CODE_OF_CONDUCT.md)** - Community guidelines
- **[Security Policy](../.github/SECURITY.md)** - Reporting security vulnerabilities

## 🏗️ Architecture Overview

Locale is organized into two main projects:

### Locale (Core Library)

The core library provides:
- **Models**: Data structures for localization files and reports
- **Formats**: 11 format handlers (JSON, YAML, RESX, PO, XLIFF, SRT, VTT, CSV, i18next, Fluent FTL, VB)
- **Services**: Business logic for scanning, diffing, checking, converting, generating, watching, and translating

**Key Design Principles:**
- Format-agnostic core
- Extensible format registry
- Minimal dependencies (only YamlDotNet)
- Strong typing with nullable reference types
- Immutable data models where appropriate

### Locale.CLI (Command-Line Tool)

The CLI tool provides:
- **Commands**: 7 commands for different operations
- **Rich console output**: Using Spectre.Console
- **Cross-platform**: Distributed via NuGet and npm
- **User-friendly**: Helpful error messages and examples

## 🔧 Development Setup

### Prerequisites

- .NET 10.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Taiizor/Locale.git
cd Locale

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack --configuration Release
```

### Project Structure

```
Locale/
├── src/
│   ├── Locale/                 # Core library
│   │   ├── Models/             # Data models
│   │   ├── Formats/            # Format handlers
│   │   └── Services/           # Business logic
│   └── Locale.CLI/             # CLI tool
│       ├── Commands/           # CLI commands
│       └── Program.cs          # Entry point
├── tests/
│   ├── Locale.Tests/           # Core library tests
│   └── Locale.CLI.Tests/       # CLI tests
├── examples/                   # Usage examples
├── docs/                       # Documentation
└── npm/                        # npm package wrapper
```

## 📖 Key Concepts

### LocalizationFile

Represents a localization file with entries:

```csharp
public sealed class LocalizationFile
{
    public required string FilePath { get; set; }
    public string? Culture { get; set; }
    public string? Format { get; set; }
    public List<LocalizationEntry> Entries { get; init; } = [];
}
```

### LocalizationEntry

Represents a single translation entry:

```csharp
public sealed class LocalizationEntry
{
    public required string Key { get; set; }
    public string? Value { get; set; }
    public string? Comment { get; set; }
    public string? Source { get; set; }  // For XLIFF and similar formats
}
```

### ILocalizationFormat

Interface for implementing custom format handlers:

```csharp
public interface ILocalizationFormat
{
    string FormatId { get; }
    IReadOnlyList<string> SupportedExtensions { get; }
    bool CanHandle(string filePath);
    LocalizationFile Parse(Stream stream, string? filePath = null);
    void Write(LocalizationFile file, Stream stream);
}
```

### FormatRegistry

Central registry for all format handlers:

```csharp
// Get the default registry with all built-in formats
var registry = FormatRegistry.Default;

// Register a custom format
registry.Register(new MyCustomFormat());

// Get format for a file
var format = registry.GetFormatForFile("messages.properties");
```

## 🧪 Testing Guidelines

### Unit Tests

Every format handler should have comprehensive tests:

- ✅ `FormatId_Returns{FormatName}` - Test format identifier
- ✅ `CanHandle_{Extension}File_ReturnsTrue` - Test file detection
- ✅ `Parse_Valid{Format}_ReturnsCorrectEntries` - Test parsing
- ✅ `Write_CreatesValid{Format}Structure` - Test serialization
- ✅ `Roundtrip_PreservesData` - Test roundtrip fidelity

### Service Tests

Service tests should cover:

- ✅ Normal operation paths
- ✅ Edge cases (empty files, missing keys, etc.)
- ✅ Error conditions
- ✅ Multiple format support

### Example Test

```csharp
[Fact]
public void Roundtrip_PreservesData()
{
    var original = new LocalizationFile
    {
        FilePath = "test.json",
        Entries = 
        [
            new() { Key = "key1", Value = "value1" },
            new() { Key = "key2", Value = "value2" }
        ]
    };
    
    var format = new JsonLocalizationFormat();
    
    // Write
    using var writeStream = new MemoryStream();
    format.Write(original, writeStream);
    
    // Read back
    writeStream.Position = 0;
    var parsed = format.Parse(writeStream, "test.json");
    
    // Assert
    Assert.Equal(original.Count, parsed.Count);
    Assert.Equal(original.GetValue("key1"), parsed.GetValue("key1"));
    Assert.Equal(original.GetValue("key2"), parsed.GetValue("key2"));
}
```

## 🚀 Performance Considerations

### Caching

`LocalizationFile` uses lazy dictionary caching for fast lookups:

```csharp
// O(1) lookup - uses cached dictionary
var value = file.GetValue("key");
var exists = file.ContainsKey("key");

// Dictionary is cached and reused
var entries = file.EntriesByKey;
```

### Large Files

For large files (>10,000 entries):
- Use streaming where possible
- Avoid loading all files into memory at once
- Consider parallel processing for batch operations

### Format Detection

Format detection is optimized:
1. Extension-based detection first (fast)
2. Content-based detection as fallback (slower)

## 🔒 Security

### Input Validation

Always validate user input:
- File paths (prevent path traversal)
- Culture codes (validate format)
- API keys (never log or expose)

### File Handling

- Use `Path.GetFullPath()` to resolve paths
- Validate file sizes before processing
- Use appropriate file permissions

### External APIs

- Store API keys securely (environment variables)
- Implement rate limiting
- Handle timeouts and retries

## 📝 Code Style

Follow the project's `.editorconfig`:
- Use spaces, not tabs
- Indent size: 4
- Use nullable reference types
- Enable all analyzers
- Treat warnings as errors

## 🤝 Contributing

See our [Contributing Guide](../.github/CONTRIBUTING.md) for:
- How to submit pull requests
- Coding standards
- Testing requirements
- Review process

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## 🆘 Need Help?

- 📖 [Read the Examples](../examples/)
- 🐛 [Report a Bug](https://github.com/Taiizor/Locale/issues)
- 💬 [Ask a Question](https://github.com/Taiizor/Locale/discussions)
- 📧 [Contact the Maintainers](https://github.com/Taiizor)

## 🔗 Additional Resources

- [.NET Globalization and Localization](https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-and-localization)
- [Unicode CLDR](https://cldr.unicode.org/)
- [Gettext Manual](https://www.gnu.org/software/gettext/manual/)
- [XLIFF Specification](http://docs.oasis-open.org/xliff/xliff-core/v2.0/xliff-core-v2.0.html)
- [Fluent Project](https://projectfluent.org/)
- [i18next Documentation](https://www.i18next.com/)