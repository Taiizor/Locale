# Basic Usage Examples

This folder contains basic examples demonstrating core Locale functionality.

## Example 1: Scanning for Translation Gaps

```csharp
using Locale.Services;
using Locale.Models;

namespace BasicUsage;

public class ScanExample
{
    public static void Main()
    {
        // Create a scan service
        var scanService = new ScanService();
        
        // Configure scan options
        var options = new ScanOptions
        {
            BaseCulture = "en",
            TargetCultures = ["tr", "de", "fr"],
            Recursive = true,
            CheckPlaceholders = true
        };
        
        // Perform the scan
        var report = scanService.Scan("./locales", options);
        
        // Display results
        Console.WriteLine($"Scanned {report.TotalFiles} files");
        Console.WriteLine($"Base culture: {report.BaseCulture}");
        
        foreach (var result in report.Results)
        {
            Console.WriteLine($"\n{result.TargetCulture}:");
            Console.WriteLine($"  Missing keys: {result.MissingKeys.Count}");
            Console.WriteLine($"  Orphan keys: {result.OrphanKeys.Count}");
            Console.WriteLine($"  Empty values: {result.EmptyValues.Count}");
            
            if (result.MissingKeys.Count > 0)
            {
                Console.WriteLine("  Missing:");
                foreach (var key in result.MissingKeys.Take(5))
                {
                    Console.WriteLine($"    - {key}");
                }
            }
        }
        
        // Check if there are any issues
        if (report.HasIssues)
        {
            Console.WriteLine("\n⚠️ Translation gaps detected!");
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("\n✅ All translations are complete!");
        }
    }
}
```

## Example 2: Comparing Two Files

```csharp
using Locale.Services;
using Locale.Models;

namespace BasicUsage;

public class DiffExample
{
    public static void Main()
    {
        var diffService = new DiffService();
        
        // Compare two files
        var report = diffService.Diff("en.json", "tr.json");
        
        Console.WriteLine("Diff Results:");
        Console.WriteLine($"Keys only in {report.FirstFile}: {report.OnlyInFirst.Count}");
        Console.WriteLine($"Keys only in {report.SecondFile}: {report.OnlyInSecond.Count}");
        Console.WriteLine($"Empty in second file: {report.EmptyInSecond.Count}");
        Console.WriteLine($"Placeholder mismatches: {report.PlaceholderMismatches.Count}");
        
        // Show missing keys
        if (report.OnlyInFirst.Count > 0)
        {
            Console.WriteLine("\nMissing in target:");
            foreach (var key in report.OnlyInFirst)
            {
                Console.WriteLine($"  - {key}");
            }
        }
        
        // Show placeholder mismatches
        if (report.PlaceholderMismatches.Count > 0)
        {
            Console.WriteLine("\nPlaceholder mismatches:");
            foreach (var mismatch in report.PlaceholderMismatches)
            {
                Console.WriteLine($"  {mismatch.Key}:");
                Console.WriteLine($"    Expected: {string.Join(", ", mismatch.FirstPlaceholders)}");
                Console.WriteLine($"    Found: {string.Join(", ", mismatch.SecondPlaceholders)}");
            }
        }
    }
}
```

## Example 3: Validating with Rules

```csharp
using Locale.Services;
using Locale.Models;

namespace BasicUsage;

public class CheckExample
{
    public static void Main()
    {
        var checkService = new CheckService();
        
        var options = new CheckOptions
        {
            Rules = 
            [
                CheckRules.NoEmptyValues,
                CheckRules.NoDuplicateKeys,
                CheckRules.ConsistentPlaceholders,
                CheckRules.NoTrailingWhitespace
            ],
            BaseCulture = "en",
            TargetCultures = ["tr", "de"]
        };
        
        var report = checkService.Check("./locales", options);
        
        Console.WriteLine($"Checked {report.TotalFiles} files");
        Console.WriteLine($"Total violations: {report.Violations.Count}");
        
        // Group violations by rule
        var byRule = report.Violations.GroupBy(v => v.Rule);
        
        foreach (var group in byRule)
        {
            Console.WriteLine($"\n{group.Key}: {group.Count()} violations");
            foreach (var violation in group.Take(3))
            {
                Console.WriteLine($"  {violation.FilePath}");
                Console.WriteLine($"    Key: {violation.Key}");
                Console.WriteLine($"    Message: {violation.Message}");
            }
        }
        
        // Return appropriate exit code for CI/CD
        Environment.Exit(report.HasViolations ? 1 : 0);
    }
}
```

## Example 4: Converting Formats

```csharp
using Locale.Services;
using Locale.Models;

namespace BasicUsage;

public class ConvertExample
{
    public static void Main()
    {
        var convertService = new ConvertService();
        
        // Single file conversion
        convertService.Convert(
            "en.json", 
            "en.yaml", 
            new ConvertOptions { ToFormat = "yaml" }
        );
        
        // Batch conversion with directory
        convertService.ConvertDirectory(
            "./json-locales",
            "./yaml-locales",
            new ConvertOptions 
            { 
                ToFormat = "yaml",
                Recursive = true,
                Overwrite = true
            }
        );
        
        Console.WriteLine("✅ Conversion completed!");
    }
}
```

## Example 5: Generating Skeleton Files

```csharp
using Locale.Services;
using Locale.Models;

namespace BasicUsage;

public class GenerateExample
{
    public static void Main()
    {
        var generateService = new GenerateService();
        
        var options = new GenerateOptions
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            Placeholder = "@@TRANSLATE@@",
            IncludeComments = true
        };
        
        var files = generateService.Generate("./locales", "./locales", options);
        
        Console.WriteLine($"Generated {files.Count} files:");
        foreach (var file in files)
        {
            Console.WriteLine($"  - {file}");
        }
    }
}
```

## Running These Examples

### As a Console App

1. Create a new console project:
```bash
dotnet new console -n LocaleExamples
cd LocaleExamples
```

2. Add Locale package:
```bash
dotnet add package Locale
```

3. Copy one of the examples above to `Program.cs`

4. Run:
```bash
dotnet run
```

### Using CLI

Most of these operations can also be done via CLI:

```bash
# Scan
locale scan ./locales --base en --targets tr,de

# Diff
locale diff en.json tr.json

# Check
locale check ./locales --rules no-empty-values,consistent-placeholders

# Convert
locale convert en.json en.yaml

# Generate
locale generate tr --from en --in ./locales --out ./locales
```