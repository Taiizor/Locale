# 🌍 Locale

**Multi-format localization library for .NET**

[![NuGet Version](https://img.shields.io/nuget/v/Locale?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Locale)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Locale?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Locale)
[![License](https://img.shields.io/github/license/Taiizor/Locale?style=flat-square)](https://github.com/Taiizor/Locale/blob/develop/LICENSE)

---

**Locale** is a powerful .NET library for working with localization files. It supports 11 formats and provides services for scanning, diffing, validating, converting, generating, and auto-translating translation files.

## ✨ Features

- 🔍 **Scan** - Compare localization files across cultures, detect missing/orphan keys
- 📊 **Diff** - Side-by-side comparison with placeholder mismatch detection
- ✅ **Check** - Validate against configurable rules
- 🔄 **Convert** - Transform between 11 different formats
- 📝 **Generate** - Create skeleton target files from a base language
- 🌐 **Translate** - Auto-translate using 10 providers (including ChatGPT, Claude, Gemini)

## 📦 Installation

```bash
dotnet add package Locale
```

## 🚀 Quick Start

```csharp
using Locale.Services;
using Locale.Models;

// Scan for translation gaps
var scanService = new ScanService();
var report = scanService.Scan("./locales", new ScanOptions
{
    BaseCulture = "en",
    TargetCultures = ["tr", "de", "fr"]
});

Console.WriteLine($"Missing: {report.MissingKeys.Count}, Orphan: {report.OrphanKeys.Count}");

// Compare two files
var diffService = new DiffService();
var diff = diffService.Diff("en.json", "tr.json");

// Validate files
var checkService = new CheckService();
var check = checkService.Check("./locales", new CheckOptions
{
    Rules = ["no-empty-values", "consistent-placeholders"]
});

// Convert formats
var convertService = new ConvertService();
convertService.Convert("en.json", "en.yaml", new ConvertOptions { ToFormat = "yaml" });

// Generate skeleton files
var generateService = new GenerateService();
generateService.Generate("./locales", "./locales", new GenerateOptions
{
    BaseCulture = "en",
    TargetCulture = "tr"
});
```

## 📁 Supported Formats

| Format | Extensions | Description |
|--------|-----------|-------------|
| JSON | `.json` | Flat and nested JSON |
| YAML | `.yaml`, `.yml` | Flat and nested YAML |
| RESX | `.resx` | .NET XML resources |
| PO | `.po` | GNU Gettext |
| XLIFF | `.xlf`, `.xliff` | XLIFF 1.2 & 2.0 |
| SRT | `.srt` | SubRip subtitles |
| VTT | `.vtt` | WebVTT subtitles |
| CSV | `.csv` | Comma-separated values |
| i18next | `.i18n.json` | i18next nested JSON |
| Fluent | `.ftl` | Mozilla Fluent FTL |
| VB | `.vb` | VB resources (read-only) |

## 🌐 Translation Providers

| Provider | API Key | Default Model |
|----------|---------|---------------|
| Google | ❌ | - |
| DeepL | ✅ | - |
| Bing | ✅ | - |
| Yandex | ✅ | - |
| LibreTranslate | ⚪ | - |
| OpenAI | ✅ | gpt-4o-mini |
| Claude | ✅ | claude-3-5-sonnet-latest |
| Gemini | ✅ | gemini-2.0-flash |
| Azure OpenAI | ✅ | - |
| Ollama | ❌ | llama3.2 |

## 📖 Documentation

For full documentation, visit the [GitHub repository](https://github.com/Taiizor/Locale).

## 📄 License

MIT License - see [LICENSE](https://github.com/Taiizor/Locale/blob/develop/LICENSE) for details.