# Performance Best Practices

This guide provides recommendations for optimizing performance when using Locale in production environments.

## Table of Contents

- [General Guidelines](#general-guidelines)
- [Large File Optimization](#large-file-optimization)
- [Batch Operations](#batch-operations)
- [Translation Performance](#translation-performance)
- [Memory Management](#memory-management)
- [Benchmarking](#benchmarking)

---

## General Guidelines

### 1. Use Caching Wisely

`LocalizationFile` uses lazy caching for key lookups:

```csharp
// ✅ Good: Uses cached dictionary (O(1) lookup)
var file = format.Parse(stream, "en.json");
bool exists = file.ContainsKey("key"); // Fast
string? value = file.GetValue("key");  // Fast

// ❌ Avoid: Linear search through entries
var existsSlow = file.Entries.Any(e => e.Key == "key"); // O(n)
```

### 2. Reuse Service Instances

Service instances are lightweight but it's more efficient to reuse them:

```csharp
// ✅ Good: Reuse service
var scanService = new ScanService();
for (int i = 0; i < 1000; i++)
{
    var report = scanService.Scan(paths[i], options);
}

// ❌ Avoid: Creating new instance each time
for (int i = 0; i < 1000; i++)
{
    var report = new ScanService().Scan(paths[i], options);
}
```

### 3. Use Appropriate FormatRegistry

```csharp
// ✅ Good: Use default registry (singleton)
var service = new ScanService(); // Uses FormatRegistry.Default

// ⚠️ Only create custom registry if you need custom formats
var customRegistry = new FormatRegistry();
customRegistry.Register(new MyCustomFormat());
var service = new ScanService(customRegistry);
```

---

## Large File Optimization

### File Size Considerations

**Performance Characteristics by File Size:**

| File Size | Entries | Parse Time | Memory Usage | Recommendation |
|-----------|---------|------------|--------------|----------------|
| < 100 KB | < 1,000 | < 10ms | < 1 MB | Standard processing |
| 100 KB - 1 MB | 1,000 - 10,000 | 10-100ms | 1-10 MB | Consider batching |
| 1 MB - 10 MB | 10,000 - 100,000 | 100ms - 1s | 10-100 MB | Use parallel processing |
| > 10 MB | > 100,000 | > 1s | > 100 MB | Split files or use streaming |

### Handling Large Files

```csharp
// For very large files (> 10,000 entries), consider processing in batches
public void ProcessLargeFile(string path, Action<LocalizationEntry> processEntry)
{
    var format = FormatRegistry.Default.GetFormatForFile(path);
    if (format == null) return;

    using var stream = File.OpenRead(path);
    var file = format.Parse(stream, path);
    
    // Process in batches to reduce memory pressure
    const int batchSize = 1000;
    for (int i = 0; i < file.Entries.Count; i += batchSize)
    {
        var batch = file.Entries.Skip(i).Take(batchSize);
        foreach (var entry in batch)
        {
            processEntry(entry);
        }
        
        // Optional: Allow GC to collect if needed
        if (i % 10000 == 0)
        {
            GC.Collect(1, GCCollectionMode.Optimized, false);
        }
    }
}
```

### Format-Specific Optimizations

**JSON/YAML:**
- Use flat structures when possible (faster parsing)
- Deeply nested structures (> 5 levels) impact performance

**RESX:**
- Binary RESX is faster than XML RESX
- Use culture-neutral resources for better caching

**PO/Gettext:**
- Keep comments concise (they're loaded into memory)
- Use msgctxt sparingly for better performance

**CSV:**
- Multi-language CSV files are slower to parse
- Consider splitting into separate files per language

---

## Batch Operations

### Parallel Processing

Use parallel processing for batch operations on multiple files:

```csharp
// ✅ Good: Parallel processing for multiple files
var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
var results = new ConcurrentBag<ScanReport>();

Parallel.ForEach(files, new ParallelOptions 
{ 
    MaxDegreeOfParallelism = Environment.ProcessorCount 
}, file =>
{
    var service = new ScanService();
    var report = service.Scan(file, options);
    results.Add(report);
});
```

### Batch Conversion

```csharp
public void ConvertMultipleFiles(string sourceDir, string targetDir, ConvertOptions options)
{
    var service = new ConvertService();
    var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
        .Where(f => FormatRegistry.Default.IsSupported(f))
        .ToList();

    // Process in parallel
    var parallelOptions = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    };

    Parallel.ForEach(files, parallelOptions, sourceFile =>
    {
        try
        {
            var relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            var targetFile = Path.Combine(targetDir, relativePath);
            var targetDir = Path.GetDirectoryName(targetFile);
            
            if (targetDir != null)
            {
                Directory.CreateDirectory(targetDir);
            }

            service.Convert(sourceFile, targetFile, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to convert {sourceFile}: {ex.Message}");
        }
    });
}
```

---

## Translation Performance

### Parallel Translation

Translation is I/O-bound, so parallel processing can significantly improve performance:

```csharp
// ✅ Good: Parallel translation with rate limiting
var options = new TranslateOptions
{
    Provider = TranslationProvider.OpenAI,
    ApiKey = "sk-...",
    SourceLanguage = "en",
    TargetLanguage = "tr",
    DegreeOfParallelism = 5,      // 5 concurrent requests
    DelayBetweenCalls = 500,       // 500ms delay between calls
    OnlyMissing = true             // Skip existing translations
};

await translateService.TranslateAsync("./locales", options);
```

### Provider-Specific Recommendations

**Google Translate (Free):**
```csharp
// Lower parallelism to avoid rate limiting
DegreeOfParallelism = 1,
DelayBetweenCalls = 1000  // 1 second between calls
```

**DeepL:**
```csharp
// Moderate parallelism
DegreeOfParallelism = 3,
DelayBetweenCalls = 300
```

**OpenAI (Paid Tier):**
```csharp
// Higher parallelism for paid tier
DegreeOfParallelism = 10,
DelayBetweenCalls = 100
```

**Claude:**
```csharp
// Conservative parallelism
DegreeOfParallelism = 5,
DelayBetweenCalls = 200
```

**Ollama (Local):**
```csharp
// Maximum parallelism (no API limits)
DegreeOfParallelism = Environment.ProcessorCount,
DelayBetweenCalls = 0
```

### Caching Translations

```csharp
// Cache translations to avoid redundant API calls
public class CachedTranslateService
{
    private readonly TranslateService _service;
    private readonly Dictionary<string, string> _cache = new();

    public async Task TranslateWithCacheAsync(string text, TranslateOptions options)
    {
        var cacheKey = $"{options.Provider}:{options.SourceLanguage}:{options.TargetLanguage}:{text}";
        
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var result = await _service.TranslateAsync(text, options);
        _cache[cacheKey] = result;
        return result;
    }
}
```

---

## Memory Management

### Stream Management

Always use `using` statements for stream operations:

```csharp
// ✅ Good: Proper stream disposal
using var stream = File.OpenRead(path);
var file = format.Parse(stream, path);

// ❌ Avoid: Manual stream management
var stream = File.OpenRead(path);
try
{
    var file = format.Parse(stream, path);
}
finally
{
    stream.Dispose();
}
```

### Memory-Efficient Scanning

```csharp
// For large directories, process files one at a time
public ScanReport ScanMemoryEfficient(string directory, ScanOptions options)
{
    var report = new ScanReport
    {
        BaseCulture = options.BaseCulture,
        TargetCultures = options.TargetCultures
    };

    foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
    {
        if (!FormatRegistry.Default.IsSupported(file))
            continue;

        // Process file
        using var stream = File.OpenRead(file);
        var format = FormatRegistry.Default.GetFormatForFile(file);
        var locFile = format?.Parse(stream, file);
        
        // Process locFile...
        
        // File is disposed after each iteration, reducing memory usage
    }

    return report;
}
```

### Reducing Allocations

```csharp
// ✅ Good: Reuse StringBuilder
var sb = new StringBuilder();
foreach (var entry in file.Entries)
{
    sb.Clear();
    sb.Append(entry.Key).Append(": ").Append(entry.Value);
    Console.WriteLine(sb.ToString());
}

// ❌ Avoid: String concatenation in loops
foreach (var entry in file.Entries)
{
    Console.WriteLine(entry.Key + ": " + entry.Value); // Creates new strings
}
```

---

## Benchmarking

### Using BenchmarkDotNet

Add benchmarking to measure performance:

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class LocaleBenchmarks
{
    private readonly ScanService _scanService = new();
    private readonly string _testPath = "./test-locales";

    [Benchmark]
    public ScanReport ScanSmallFiles()
    {
        return _scanService.Scan(_testPath, new ScanOptions
        {
            BaseCulture = "en",
            TargetCultures = ["tr"]
        });
    }

    [Benchmark]
    public LocalizationFile ParseJson()
    {
        var format = new JsonLocalizationFormat();
        using var stream = File.OpenRead("test.json");
        return format.Parse(stream, "test.json");
    }
}

// Run benchmarks
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<LocaleBenchmarks>();
    }
}
```

### Performance Testing

```csharp
// Simple performance measurement
public static class PerformanceHelper
{
    public static TimeSpan Measure(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.Elapsed;
    }

    public static async Task<TimeSpan> MeasureAsync(Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        await action();
        sw.Stop();
        return sw.Elapsed;
    }
}

// Usage
var elapsed = PerformanceHelper.Measure(() =>
{
    var service = new ScanService();
    var report = service.Scan("./locales", options);
});

Console.WriteLine($"Scan completed in {elapsed.TotalMilliseconds}ms");
```

---

## Performance Checklist

- [ ] Use `FormatRegistry.Default` for standard scenarios
- [ ] Reuse service instances in loops
- [ ] Enable `OnlyMissing = true` for translations
- [ ] Use appropriate `DegreeOfParallelism` for translations (1-10)
- [ ] Add delays between API calls to avoid rate limiting
- [ ] Process large files in batches (> 10,000 entries)
- [ ] Use parallel processing for multiple files
- [ ] Dispose streams properly with `using`
- [ ] Cache translations when possible
- [ ] Monitor memory usage for large operations
- [ ] Use `SearchOption.TopDirectoryOnly` when recursive scan isn't needed
- [ ] Profile with BenchmarkDotNet for critical paths
- [ ] Consider file size limits (warn at > 10 MB)
- [ ] Use `StringComparison.Ordinal` for case-sensitive comparisons

---

## Real-World Performance Examples

### Example 1: Large Monorepo

**Scenario:** 500 localization files, 10,000+ keys per language, 20 languages

**Optimization Strategy:**
```csharp
var options = new ScanOptions
{
    BaseCulture = "en",
    TargetCultures = ["tr", "de", "fr", "es"], // Focus on specific languages
    Recursive = true,
    CheckPlaceholders = false, // Disable if not needed
    IgnorePatterns = ["node_modules/**", "build/**"] // Skip build artifacts
};

// Use parallel scanning
var parallelOptions = new ParallelOptions 
{ 
    MaxDegreeOfParallelism = 4 
};

var directories = new[] { "./app", "./web", "./mobile" };
var reports = new ConcurrentBag<ScanReport>();

Parallel.ForEach(directories, parallelOptions, dir =>
{
    var service = new ScanService();
    var report = service.Scan(dir, options);
    reports.Add(report);
});
```

**Results:** Reduced from 45s (sequential) to 12s (parallel)

### Example 2: CI/CD Pipeline

**Scenario:** Check 100 files on every commit

**Optimization Strategy:**
```csharp
// Run check in parallel with specific rules
var service = new CheckService();
var files = Directory.GetFiles("./locales", "*.*", SearchOption.AllDirectories)
    .Where(f => FormatRegistry.Default.IsSupported(f))
    .ToList();

var reports = new ConcurrentBag<CheckReport>();
Parallel.ForEach(files, file =>
{
    var report = service.Check(file, new CheckOptions
    {
        Rules = ["no-empty-values", "no-duplicate-keys"], // Only essential rules
        Recursive = false // Already have file list
    });
    reports.Add(report);
});

var totalViolations = reports.Sum(r => r.Violations.Count);
Environment.Exit(totalViolations > 0 ? 1 : 0);
```

**Results:** Reduced from 8s (sequential) to 2s (parallel)

### Example 3: Translation Automation

**Scenario:** Translate 5,000 missing keys from English to Turkish

**Optimization Strategy:**
```csharp
var options = new TranslateOptions
{
    Provider = TranslationProvider.OpenAI,
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    Model = "gpt-4o-mini", // Faster and cheaper
    SourceLanguage = "en",
    TargetLanguage = "tr",
    OnlyMissing = true,
    DegreeOfParallelism = 8,  // 8 concurrent requests
    DelayBetweenCalls = 200   // 200ms delay
};

await translateService.TranslateAsync("./locales", options);
```

**Results:** Reduced from 50 minutes (sequential) to 8 minutes (parallel)

---

## Monitoring and Profiling

### Application Insights Integration

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public class TelemetryService
{
    private readonly TelemetryClient _telemetry = new();

    public void TrackScan(string path, ScanOptions options, TimeSpan duration, ScanReport report)
    {
        _telemetry.TrackEvent("Scan", new Dictionary<string, string>
        {
            ["Path"] = path,
            ["BaseCulture"] = options.BaseCulture,
            ["TargetCultures"] = string.Join(",", options.TargetCultures),
            ["Duration"] = duration.TotalSeconds.ToString(),
            ["TotalMissing"] = report.Results.Sum(r => r.MissingKeys.Count).ToString()
        });
    }
}
```

### Custom Performance Counters

```csharp
public class PerformanceMonitor
{
    private readonly PerformanceCounter _scanCounter;
    private readonly PerformanceCounter _parseCounter;

    public PerformanceMonitor()
    {
        _scanCounter = new PerformanceCounter("Locale", "Scans/sec", readOnly: false);
        _parseCounter = new PerformanceCounter("Locale", "Files Parsed/sec", readOnly: false);
    }

    public void RecordScan() => _scanCounter.Increment();
    public void RecordParse() => _parseCounter.Increment();
}
```

---

## Additional Resources

- [.NET Performance Tips](https://learn.microsoft.com/en-us/dotnet/core/performance/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Memory Management Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/memory-management-and-gc)
- [Parallel Programming in .NET](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/)