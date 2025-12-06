# Locale API Reference

This document provides comprehensive API documentation for the Locale library.

## Table of Contents

- [Core Models](#core-models)
- [Format Interfaces](#format-interfaces)
- [Service Classes](#service-classes)
- [CLI Commands](#cli-commands)
- [Translation Providers](#translation-providers)

---

## Core Models

### LocalizationFile

Represents a localization file containing multiple entries for a specific culture.

```csharp
public sealed class LocalizationFile
{
    public required string FilePath { get; set; }
    public string? Culture { get; set; }
    public string? Format { get; set; }
    public List<LocalizationEntry> Entries { get; init; } = [];
    
    // Quick lookup methods
    public string? GetValue(string key);
    public bool ContainsKey(string key);
    public CultureInfo? GetCultureInfo();
}
```

**Properties:**
- `FilePath` - The file path (absolute or relative)
- `Culture` - Detected or configured culture (e.g., "en", "tr", "de")
- `Format` - Format of this file (e.g., "json", "yaml", "resx")
- `Entries` - Collection of localization entries
- `EntriesByKey` - Dictionary mapping keys to entries (cached for performance)
- `Keys` - All keys in this file
- `Count` - Number of entries

**Methods:**
- `GetValue(string key)` - Gets the value for a specific key, or null if not found
- `ContainsKey(string key)` - Determines whether this file contains the specified key
- `GetCultureInfo()` - Attempts to get culture information from the Culture string

**Example:**
```csharp
var file = new LocalizationFile
{
    FilePath = "en.json",
    Culture = "en",
    Entries = 
    [
        new() { Key = "welcome", Value = "Welcome!" },
        new() { Key = "goodbye", Value = "Goodbye!" }
    ]
};

string? greeting = file.GetValue("welcome"); // "Welcome!"
bool exists = file.ContainsKey("hello"); // false
```

### LocalizationEntry

Represents a single translation entry.

```csharp
public sealed class LocalizationEntry
{
    public required string Key { get; set; }
    public string? Value { get; set; }
    public string? Comment { get; set; }
    public string? Source { get; set; }
}
```

**Properties:**
- `Key` - Unique identifier for the translation
- `Value` - Translated text
- `Comment` - Optional comment or note
- `Source` - Source text (used in XLIFF and similar formats)

**Example:**
```csharp
var entry = new LocalizationEntry
{
    Key = "app.title",
    Value = "My Application",
    Comment = "Main application title displayed in header"
};
```

### ScanReport

Contains results from scanning localization files.

```csharp
public sealed class ScanReport
{
    public required string BaseCulture { get; init; }
    public List<string> TargetCultures { get; init; } = [];
    public List<CultureComparisonResult> Results { get; init; } = [];
}

public sealed class CultureComparisonResult
{
    public required string Culture { get; init; }
    public List<string> MissingKeys { get; init; } = [];
    public List<string> OrphanKeys { get; init; } = [];
    public List<string> EmptyValues { get; init; } = [];
    public List<PlaceholderMismatch> PlaceholderMismatches { get; init; } = [];
}
```

**Example:**
```csharp
var scanService = new ScanService();
var report = scanService.Scan("./locales", new ScanOptions 
{ 
    BaseCulture = "en",
    TargetCultures = ["tr", "de"]
});

foreach (var result in report.Results)
{
    Console.WriteLine($"Culture: {result.Culture}");
    Console.WriteLine($"  Missing: {result.MissingKeys.Count}");
    Console.WriteLine($"  Orphan: {result.OrphanKeys.Count}");
}
```

### DiffReport

Contains results from comparing two files.

```csharp
public sealed class DiffReport
{
    public List<string> OnlyInFirst { get; init; } = [];
    public List<string> OnlyInSecond { get; init; } = [];
    public List<string> EmptyInSecond { get; init; } = [];
    public List<PlaceholderMismatch> PlaceholderMismatches { get; init; } = [];
}
```

### CheckReport

Contains validation results from checking files against rules.

```csharp
public sealed class CheckReport
{
    public List<CheckViolation> Violations { get; init; } = [];
    public bool HasViolations => Violations.Count > 0;
}

public sealed class CheckViolation
{
    public required string Rule { get; init; }
    public required string FilePath { get; init; }
    public string? Key { get; init; }
    public required string Message { get; init; }
}
```

---

## Format Interfaces

### ILocalizationFormat

Base interface for all format handlers.

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

**Members:**
- `FormatId` - Unique identifier for this format (e.g., "json", "yaml")
- `SupportedExtensions` - File extensions supported by this format
- `CanHandle(filePath)` - Determines if this format can handle a file
- `Parse(stream, filePath)` - Parses a stream into a LocalizationFile
- `Write(file, stream)` - Writes a LocalizationFile to a stream

**Built-in Formats:**
- JSON (`.json`)
- YAML (`.yaml`, `.yml`)
- RESX (`.resx`)
- PO/Gettext (`.po`)
- XLIFF (`.xlf`, `.xliff`)
- SRT (`.srt`)
- WebVTT (`.vtt`)
- CSV (`.csv`)
- i18next JSON (`.i18n.json`)
- Fluent FTL (`.ftl`)
- VB Resources (`.vb`) - read-only

### LocalizationFormatBase

Abstract base class providing common functionality.

```csharp
public abstract class LocalizationFormatBase : ILocalizationFormat
{
    public abstract string FormatId { get; }
    public abstract IReadOnlyList<string> SupportedExtensions { get; }
    public virtual bool CanHandle(string filePath);
    public abstract LocalizationFile Parse(Stream stream, string? filePath = null);
    public abstract void Write(LocalizationFile file, Stream stream);
}
```

**Creating a Custom Format:**
```csharp
public sealed class MyCustomFormat : LocalizationFormatBase
{
    public override string FormatId => "myformat";
    public override IReadOnlyList<string> SupportedExtensions => [".myext"];

    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        // Implement parsing logic
        var entries = new List<LocalizationEntry>();
        // ... parse stream
        return new LocalizationFile
        {
            FilePath = filePath ?? "unknown",
            Format = FormatId,
            Entries = entries
        };
    }

    public override void Write(LocalizationFile file, Stream stream)
    {
        // Implement writing logic
        using var writer = new StreamWriter(stream);
        foreach (var entry in file.Entries)
        {
            // ... write entry
        }
    }
}
```

### FormatRegistry

Central registry for all format handlers.

```csharp
public sealed class FormatRegistry
{
    public static FormatRegistry Default { get; }
    public IReadOnlyList<ILocalizationFormat> Formats { get; }
    
    public void Register(ILocalizationFormat format);
    public ILocalizationFormat? GetFormat(string formatId);
    public ILocalizationFormat? GetFormatForFile(string filePath);
    public bool IsSupported(string filePath);
    public IEnumerable<string> GetSupportedExtensions();
}
```

**Example:**
```csharp
// Get default registry with all built-in formats
var registry = FormatRegistry.Default;

// Register custom format
registry.Register(new MyCustomFormat());

// Get format for a file
var format = registry.GetFormatForFile("messages.properties");

// Check if file is supported
bool supported = registry.IsSupported("translations.xlf");
```

---

## Service Classes

### ScanService

Scans and compares localization files across cultures.

```csharp
public sealed class ScanService
{
    public ScanService();
    public ScanService(FormatRegistry registry);
    
    public ScanReport Scan(string path, ScanOptions options);
}
```

**ScanOptions:**
```csharp
public sealed class ScanOptions
{
    public required string BaseCulture { get; set; }
    public List<string> TargetCultures { get; set; } = [];
    public bool Recursive { get; set; } = true;
    public List<string> IgnorePatterns { get; set; } = [];
    public bool CheckPlaceholders { get; set; } = true;
    public string PlaceholderPattern { get; set; } = PlaceholderHelper.DefaultPlaceholderPattern;
}
```

**Example:**
```csharp
var service = new ScanService();
var report = service.Scan("./locales", new ScanOptions
{
    BaseCulture = "en",
    TargetCultures = ["tr", "de", "fr"],
    Recursive = true,
    CheckPlaceholders = true
});

foreach (var result in report.Results)
{
    Console.WriteLine($"\n{result.Culture}:");
    
    if (result.MissingKeys.Any())
        Console.WriteLine($"  Missing: {string.Join(", ", result.MissingKeys.Take(5))}");
    
    if (result.OrphanKeys.Any())
        Console.WriteLine($"  Orphan: {string.Join(", ", result.OrphanKeys.Take(5))}");
}
```

### DiffService

Compares two localization files.

```csharp
public sealed class DiffService
{
    public DiffService();
    public DiffService(FormatRegistry registry);
    
    public DiffReport Diff(string firstPath, string secondPath, DiffOptions? options = null);
}
```

**DiffOptions:**
```csharp
public sealed class DiffOptions
{
    public bool CheckPlaceholders { get; set; } = true;
    public string PlaceholderPattern { get; set; } = PlaceholderHelper.DefaultPlaceholderPattern;
}
```

**Example:**
```csharp
var service = new DiffService();
var report = service.Diff("en.json", "tr.json");

Console.WriteLine($"Only in first: {report.OnlyInFirst.Count}");
Console.WriteLine($"Only in second: {report.OnlyInSecond.Count}");
Console.WriteLine($"Empty in second: {report.EmptyInSecond.Count}");
```

### CheckService

Validates localization files against configurable rules.

```csharp
public sealed class CheckService
{
    public CheckService();
    public CheckService(FormatRegistry registry);
    
    public CheckReport Check(string path, CheckOptions options);
}
```

**CheckOptions:**
```csharp
public sealed class CheckOptions
{
    public List<string> Rules { get; set; } = [];
    public bool Recursive { get; set; } = true;
    public string? BaseCulture { get; set; }
    public List<string> TargetCultures { get; set; } = [];
}
```

**Available Rules:**
- `no-empty-values` - Flag keys with empty or whitespace-only values
- `no-duplicate-keys` - Flag duplicate keys in a file
- `no-orphan-keys` - Flag keys that exist in target but not in base
- `consistent-placeholders` - Ensure placeholders match between cultures
- `no-trailing-whitespace` - Flag values with trailing whitespace

**Example:**
```csharp
var service = new CheckService();
var report = service.Check("./locales", new CheckOptions
{
    Rules = ["no-empty-values", "consistent-placeholders"],
    Recursive = true
});

if (report.HasViolations)
{
    foreach (var violation in report.Violations)
    {
        Console.WriteLine($"[{violation.Rule}] {violation.FilePath}");
        Console.WriteLine($"  {violation.Message}");
    }
}
```

### ConvertService

Converts localization files between formats.

```csharp
public sealed class ConvertService
{
    public ConvertService();
    public ConvertService(FormatRegistry registry);
    
    public void Convert(string inputPath, string outputPath, ConvertOptions? options = null);
}
```

**ConvertOptions:**
```csharp
public sealed class ConvertOptions
{
    public string? FromFormat { get; set; }
    public string? ToFormat { get; set; }
    public string? Culture { get; set; }
    public bool Force { get; set; }
}
```

**Example:**
```csharp
var service = new ConvertService();

// Single file
service.Convert("en.json", "en.yaml", new ConvertOptions
{
    ToFormat = "yaml"
});

// Batch conversion
service.Convert("en.resx", "messages.en.po", new ConvertOptions
{
    ToFormat = "po",
    Force = true
});
```

### GenerateService

Generates skeleton target files from a base language.

```csharp
public sealed class GenerateService
{
    public GenerateService();
    public GenerateService(FormatRegistry registry);
    
    public void Generate(string inputPath, string outputPath, GenerateOptions options);
}
```

**GenerateOptions:**
```csharp
public sealed class GenerateOptions
{
    public required string BaseCulture { get; set; }
    public required string TargetCulture { get; set; }
    public string Placeholder { get; set; } = "@@MISSING@@";
    public bool UseEmpty { get; set; }
    public bool SkipExisting { get; set; } = true;
}
```

**Example:**
```csharp
var service = new GenerateService();
service.Generate("./locales", "./locales", new GenerateOptions
{
    BaseCulture = "en",
    TargetCulture = "tr",
    Placeholder = "@@TRANSLATE@@",
    SkipExisting = true
});
```

### TranslateService

Auto-translates localization files using external APIs.

```csharp
public sealed class TranslateService
{
    public TranslateService();
    public TranslateService(FormatRegistry registry);
    
    public Task TranslateAsync(string path, TranslateOptions options, CancellationToken cancellationToken = default);
}
```

**TranslateOptions:**
```csharp
public sealed class TranslateOptions
{
    public TranslationProvider Provider { get; set; } = TranslationProvider.Google;
    public string? ApiKey { get; set; }
    public string? ApiEndpoint { get; set; }
    public required string SourceLanguage { get; set; }
    public required string TargetLanguage { get; set; }
    public bool OverwriteExisting { get; set; }
    public bool OnlyMissing { get; set; } = true;
    public bool Recursive { get; set; } = true;
    public int DelayBetweenCalls { get; set; } = 100;
    public string? Model { get; set; }
    public int DegreeOfParallelism { get; set; } = 1;
}
```

**Example:**
```csharp
var service = new TranslateService();

// Free translation with Google
await service.TranslateAsync("./locales", new TranslateOptions
{
    Provider = TranslationProvider.Google,
    SourceLanguage = "en",
    TargetLanguage = "tr",
    OnlyMissing = true
});

// AI translation with ChatGPT
await service.TranslateAsync("./locales", new TranslateOptions
{
    Provider = TranslationProvider.OpenAI,
    ApiKey = "sk-...",
    Model = "gpt-4o-mini",
    SourceLanguage = "en",
    TargetLanguage = "de",
    DegreeOfParallelism = 5, // Parallel processing
    DelayBetweenCalls = 500
});
```

### WatchService

Monitors localization files for changes and re-runs operations.

```csharp
public sealed class WatchService
{
    public WatchService();
    public WatchService(FormatRegistry registry, ScanService? scanService = null, CheckService? checkService = null);
    
    public void Watch(string path, WatchOptions options);
}
```

**WatchOptions:**
```csharp
public sealed class WatchOptions
{
    public WatchMode Mode { get; set; } = WatchMode.Scan;
    public ScanOptions? ScanOptions { get; set; }
    public CheckOptions? CheckOptions { get; set; }
}

public enum WatchMode
{
    Scan,
    Check
}
```

**Example:**
```csharp
var service = new WatchService();
service.Watch("./locales", new WatchOptions
{
    Mode = WatchMode.Scan,
    ScanOptions = new()
    {
        BaseCulture = "en",
        TargetCultures = ["tr", "de"]
    }
});
```

---

## Translation Providers

### Supported Providers

| Provider | API Key Required | Default Model | Best For |
|----------|------------------|---------------|----------|
| Google | ❌ No | - | Quick, free translations |
| DeepL | ✅ Yes | - | High-quality European languages |
| Bing | ✅ Yes | - | Microsoft ecosystem |
| Yandex | ✅ Yes | - | Slavic languages |
| LibreTranslate | ⚪ Optional | - | Self-hosted, privacy |
| OpenAI | ✅ Yes | `gpt-4o-mini` | Context-aware AI |
| Claude | ✅ Yes | `claude-3-5-sonnet-latest` | Nuanced translations |
| Gemini | ✅ Yes | `gemini-2.0-flash` | Fast AI translations |
| Azure OpenAI | ✅ Yes | - | Enterprise deployments |
| Ollama | ❌ No | `llama3.2` | Local, private LLM |

### Provider Configuration

**Google Translate:**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.Google,
    SourceLanguage = "en",
    TargetLanguage = "tr"
};
```

**DeepL:**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.DeepL,
    ApiKey = "your-deepl-api-key",
    SourceLanguage = "en",
    TargetLanguage = "de"
};
```

**OpenAI ChatGPT:**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.OpenAI,
    ApiKey = "sk-...",
    Model = "gpt-4o-mini", // or "gpt-4", "gpt-3.5-turbo"
    SourceLanguage = "en",
    TargetLanguage = "fr",
    DegreeOfParallelism = 5
};
```

**Claude:**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.Claude,
    ApiKey = "your-claude-api-key",
    Model = "claude-3-5-sonnet-latest",
    SourceLanguage = "en",
    TargetLanguage = "es"
};
```

**Ollama (Local):**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.Ollama,
    ApiEndpoint = "http://localhost:11434", // default
    Model = "llama3.2", // or any installed model
    SourceLanguage = "en",
    TargetLanguage = "it"
};
```

---

## Helper Classes

### PathHelper

Provides utilities for path manipulation and culture detection.

```csharp
public static class PathHelper
{
    public static string? DetectCultureFromPath(string filePath);
    public static string GetCultureSuffix(string culture);
    public static bool TryParseCultureFromPath(string filePath, out string? culture);
}
```

### PlaceholderHelper

Provides utilities for detecting and comparing placeholders.

```csharp
public static class PlaceholderHelper
{
    public const string DefaultPlaceholderPattern = @"\{[^}]+\}|\{\{[^}]+\}\}";
    
    public static List<string> ExtractPlaceholders(string? text, string pattern);
    public static bool HasPlaceholderMismatch(string? text1, string? text2, string pattern);
}
```

---

## Error Handling

All services throw standard .NET exceptions:

- `ArgumentException` / `ArgumentNullException` - Invalid arguments
- `FileNotFoundException` / `DirectoryNotFoundException` - Missing files/directories
- `InvalidOperationException` - Invalid file format or state
- `NotSupportedException` - Unsupported operations (e.g., writing VB format)

**Example:**
```csharp
try
{
    var service = new ScanService();
    var report = service.Scan("./locales", options);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid argument: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
}
```

---

## Threading and Async

- All I/O operations are synchronous by default
- `TranslateService` provides async methods with `CancellationToken` support
- Services are thread-safe for read operations
- For concurrent operations, create separate service instances

**Async Example:**
```csharp
using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(5));

try
{
    await translateService.TranslateAsync(path, options, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Translation cancelled or timed out");
}
```

---

## Best Practices

1. **Use FormatRegistry.Default** for most scenarios
2. **Cache LocalizationFile** results when possible (they use internal caching)
3. **Set appropriate DegreeOfParallelism** for translation (5-10 for most APIs)
4. **Use CheckService in CI/CD** with specific rules
5. **Always handle exceptions** appropriately
6. **Use CancellationToken** for long-running operations
7. **Validate input paths** before processing

---

## Additional Resources

- [Main README](../README.md)
- [Error Handling Guidelines](./ERROR-HANDLING.md)
- [Examples Directory](../examples/)
- [Contributing Guide](../.github/CONTRIBUTING.md)