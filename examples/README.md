# Locale Examples

This directory contains practical examples demonstrating how to use Locale in real-world scenarios.

## 📁 Examples Overview

### 1. [Basic Usage](./basic-usage/) 
Learn the fundamentals of using Locale library and CLI in your .NET projects.
- Scanning translation files
- Comparing files with Diff
- Basic validation with Check command

### 2. [Format Conversion](./format-conversion/)
Examples of converting between different localization formats.
- JSON ↔ YAML
- RESX ↔ PO (Gettext)
- XLIFF ↔ JSON
- Batch conversion workflows

### 3. [CI/CD Integration](./ci-integration/)
Integrate Locale into your continuous integration pipeline.
- GitHub Actions workflow
- Azure Pipelines configuration
- Pre-commit hooks
- Automated quality checks

### 4. [Custom Format](./custom-format/)
Create and register your own localization format handler.
- Implementing `ILocalizationFormat`
- Registering custom formats
- Testing custom formats

## 🚀 Quick Start

### Using as a Library

```csharp
using Locale.Services;
using Locale.Models;

// Scan for missing translations
var scanService = new ScanService();
var report = scanService.Scan("./locales", new ScanOptions 
{ 
    BaseCulture = "en",
    TargetCultures = ["tr", "de"]
});

Console.WriteLine($"Missing keys: {report.MissingKeys.Count}");
```

### Using the CLI

```bash
# Install globally
npm install -g @taiizor/locale-cli
# or
dotnet tool install -g Locale.CLI

# Scan for issues
locale scan ./locales --base en --targets tr,de

# Convert formats
locale convert en.json en.yaml

# Validate with rules
locale check ./locales --rules no-empty-values,consistent-placeholders --ci
```

## 📚 Additional Resources

- [Main Documentation](../README.md)
- [API Reference](../docs/) (coming soon)
- [Contributing Guide](../.github/CONTRIBUTING.md)
- [Changelog](../CHANGELOG.md)

## 💡 Need Help?

- 🐛 [Report a Bug](https://github.com/Taiizor/Locale/issues)
- 💬 [Ask a Question](https://github.com/Taiizor/Locale/discussions)
- 📖 [Read the Docs](../README.md)