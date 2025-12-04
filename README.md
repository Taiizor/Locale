<div align="center">
  <img src=".images/Logo.png" alt="Locale Logo" width="64" height="64">
  <h1>Locale</h1>
  <p><strong>Multi-format localization library and CLI tool for .NET</strong></p>
  <p>Scan, diff, validate, convert, generate, watch, and auto-translate translation files across 11 formats</p>

  <p>
    <a href="https://github.com/Taiizor/Locale/actions"><img src="https://img.shields.io/github/actions/workflow/status/Taiizor/Locale/build.yml?style=flat-square&logo=github" alt="Build Status"></a>
    <a href="https://www.nuget.org/packages/Locale"><img src="https://img.shields.io/nuget/v/Locale?style=flat-square&logo=nuget" alt="NuGet Version"></a>
    <a href="https://www.nuget.org/packages/Locale"><img src="https://img.shields.io/nuget/dt/Locale?style=flat-square&logo=nuget" alt="NuGet Downloads"></a>
    <a href="https://www.npmjs.com/package/@taiizor/locale-cli"><img src="https://img.shields.io/npm/v/@taiizor/locale-cli?style=flat-square&logo=npm" alt="npm Version"></a>
    <a href="https://www.npmjs.com/package/@taiizor/locale-cli"><img src="https://img.shields.io/npm/dm/@taiizor/locale-cli?style=flat-square&logo=npm" alt="npm Downloads"></a>
    <a href="https://github.com/Taiizor/Locale/blob/develop/LICENSE"><img src="https://img.shields.io/github/license/Taiizor/Locale?style=flat-square" alt="License"></a>
  </p>

  <p>
    <a href="#-installation">Installation</a> •
    <a href="#-cli-commands">CLI Commands</a> •
    <a href="#-library-usage">Library Usage</a> •
    <a href="#-supported-formats">Formats</a> •
    <a href="#-translation-providers">Providers</a>
  </p>
</div>

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔍 **Scan** | Compare localization files across cultures, detect missing/orphan keys and empty values |
| 📊 **Diff** | Side-by-side comparison of two files with placeholder mismatch detection |
| ✅ **Check** | Validate against configurable rules with CI/CD exit codes |
| 🔄 **Convert** | Transform between 11 different localization formats |
| 📝 **Generate** | Create skeleton target files from a base language |
| 👁️ **Watch** | File system watcher that auto-runs scan/check on changes |
| 🌐 **Translate** | Auto-translate using 10 providers including AI (ChatGPT, Claude, Gemini) |

## 📦 Installation

### CLI Tool

#### npm / pnpm / bun / yarn

```bash
# npm
npm install -g @taiizor/locale-cli

# pnpm
pnpm add -g @taiizor/locale-cli

# bun
bun add -g @taiizor/locale-cli

# yarn
yarn global add @taiizor/locale-cli
```

#### .NET Tool (Global)

```bash
dotnet tool install -g Locale.CLI
```

### Library (NuGet Package)

```bash
dotnet add package Locale
```

### From Source

```bash
git clone https://github.com/Taiizor/Locale.git
cd Locale
dotnet build
```

## 🖥️ CLI Commands

### `locale scan` - Find Translation Gaps

```bash
# Compare English base to Turkish target
locale scan ./locales --base en --targets tr

# Multiple target cultures with JSON report
locale scan ./locales --base en --targets tr,de,fr --output report.json

# Recursive scanning with format filter
locale scan ./locales --base en --recursive --format json
```

### `locale diff` - Compare Two Files

```bash
# Compare files (auto-detects format)
locale diff en.json tr.json

# Cross-format comparison
locale diff en.json tr.po --output diff.json
```

### `locale check` - Validate & Lint

```bash
# Check with all rules
locale check ./locales

# Specific rules with CI exit codes
locale check ./locales --rules no-empty-values,consistent-placeholders --ci
```

<details>
<summary><strong>📋 Available Validation Rules</strong></summary>

| Rule | Description |
|------|-------------|
| `no-empty-values` | Flag keys with empty or whitespace-only values |
| `no-duplicate-keys` | Flag duplicate keys in a file |
| `no-orphan-keys` | Flag keys that exist in target but not in base |
| `consistent-placeholders` | Ensure placeholders match between cultures |
| `no-trailing-whitespace` | Flag values with trailing whitespace |

</details>

### `locale convert` - Transform Formats

```bash
# Single file conversion
locale convert en.json en.yaml

# Directory batch conversion
locale convert ./json-locales ./yaml-locales --to yaml --force
```

### `locale generate` - Create Skeleton Files

```bash
# Generate Turkish skeleton from English
locale generate tr --from en --in ./locales --out ./locales

# Generate with empty values
locale generate de --from en --in ./locales --empty
```

### `locale watch` - Monitor Changes

```bash
# Watch and auto-scan
locale watch ./locales --base en --mode scan

# Watch and auto-check
locale watch ./locales --mode check --targets tr,de
```

### `locale translate` - Auto-Translate

```bash
# Google Translate (free, no API key)
locale translate tr --from en --in ./locales --provider google

# AI Translation with ChatGPT
locale translate tr --from en --in ./locales --provider openai --api-key YOUR_KEY

# Local LLM with Ollama
locale translate tr --from en --in ./locales --provider ollama --model llama3.2

# Parallel translation (5 concurrent requests with 500ms delay)
locale translate tr --from en --in ./locales --parallel 5 --delay 500
```

<details>
<summary><strong>⚡ Parallel Translation Options</strong></summary>

| Option | Default | Description |
|--------|---------|-------------|
| `--parallel` | `1` | Degree of parallelism (1 = sequential, higher = faster) |
| `--delay` | `100` | Delay between API calls in milliseconds (for rate limiting) |

**Tips:**
- Use `--parallel 5-10` for faster translations on large files
- Increase `--delay` if you hit API rate limits
- Sequential mode (`--parallel 1`) is safest for strict rate-limited APIs

</details>

## 🌐 Translation Providers

| Provider | API Key | Default Model | Best For |
|----------|---------|---------------|----------|
| 🔵 **Google** | ❌ No | - | Quick, free translations |
| 🟣 **DeepL** | ✅ Yes | - | High-quality European languages |
| 🔷 **Bing** | ✅ Yes | - | Microsoft ecosystem |
| 🟡 **Yandex** | ✅ Yes | - | Slavic languages |
| 🟢 **LibreTranslate** | ⚪ Optional | - | Self-hosted, privacy |
| 🤖 **OpenAI** | ✅ Yes | `gpt-4o-mini` | Context-aware AI translation |
| 🧠 **Claude** | ✅ Yes | `claude-3-5-sonnet-latest` | Nuanced translations |
| ✨ **Gemini** | ✅ Yes | `gemini-2.0-flash` | Fast AI translations |
| ☁️ **Azure OpenAI** | ✅ Yes | - | Enterprise deployments |
| 🦙 **Ollama** | ❌ No | `llama3.2` | Local, private LLM |

## 📁 Supported Formats

| Format | Extensions | Description |
|--------|-----------|-------------|
| 📄 JSON | `.json` | Flat and nested JSON structures |
| 📝 YAML | `.yaml`, `.yml` | Flat and nested YAML structures |
| 🔧 RESX | `.resx` | .NET XML resource files |
| 📋 PO | `.po` | GNU Gettext translation files |
| 🔀 XLIFF | `.xlf`, `.xliff` | XML Localization Interchange (1.2 & 2.0) |
| 🎬 SRT | `.srt` | SubRip subtitle files |
| 📺 VTT | `.vtt` | WebVTT subtitle files |
| 📊 CSV | `.csv` | Comma-separated values |
| 🌍 i18next | `.i18n.json` | i18next-style nested JSON |
| 🦊 Fluent | `.ftl` | Mozilla Fluent FTL files |
| 🔷 VB | `.vb` | Visual Basic resource wrappers (read-only) |

## 📚 Library Usage

You can also use Locale as a library in your .NET projects:

```csharp
using Locale.Formats;
using Locale.Services;
using Locale.Models;

// 🔍 Scan for translation gaps
var scanService = new ScanService();
var scanReport = scanService.Scan("./locales", new ScanOptions 
{ 
    BaseCulture = "en",
    TargetCultures = ["tr", "de", "fr"]
});

Console.WriteLine($"Missing keys: {scanReport.MissingKeys.Count}");
Console.WriteLine($"Orphan keys: {scanReport.OrphanKeys.Count}");

// 📊 Diff two files
var diffService = new DiffService();
var diffReport = diffService.Diff("en.json", "tr.json");

foreach (var key in diffReport.OnlyInFirst)
    Console.WriteLine($"Missing in target: {key}");

// ✅ Check for violations
var checkService = new CheckService();
var checkReport = checkService.Check("./locales", new CheckOptions
{
    Rules = [CheckRules.NoEmptyValues, CheckRules.ConsistentPlaceholders]
});

if (checkReport.HasViolations)
    Console.WriteLine($"Found {checkReport.Violations.Count} issues");

// 🔄 Convert between formats
var convertService = new ConvertService();
convertService.Convert("en.json", "en.yaml", new ConvertOptions { ToFormat = "yaml" });
convertService.Convert("en.json", "messages.en.po", new ConvertOptions { ToFormat = "po" });

// 📝 Generate skeleton files
var generateService = new GenerateService();
generateService.Generate("./locales", "./locales", new GenerateOptions
{
    BaseCulture = "en",
    TargetCulture = "tr",
    Placeholder = "@@TRANSLATE@@"
});

// 🌐 Auto-translate (requires API setup)
var translateService = new TranslateService();
await translateService.TranslateAsync("./locales", new TranslateOptions
{
    SourceCulture = "en",
    TargetCulture = "tr",
    Provider = "openai",
    ApiKey = "your-api-key"
});

// 📂 Parse specific formats
var jsonFormat = new JsonLocalizationFormat();
var file = jsonFormat.Parse("en.json");

foreach (var entry in file.Entries)
    Console.WriteLine($"{entry.Key} = {entry.Value}");
```

## 🏗️ Project Structure

```
Locale/
├── src/
│   ├── Locale/                 # 📚 Core library (NuGet: Locale)
│   │   ├── Models/             #    Data models
│   │   ├── Formats/            #    11 format handlers
│   │   └── Services/           #    Business logic
│   └── Locale.CLI/             # 🖥️ CLI tool (NuGet: Locale.CLI)
│       └── Commands/           #    7 CLI commands
└── tests/
    ├── Locale.Tests/           # ✅ Core library tests
    └── Locale.CLI.Tests/       # ✅ CLI tests
```

## 🔧 Extending with Custom Formats

To add support for a new localization format:

```csharp
using Locale.Formats;
using Locale.Models;

public sealed class MyCustomFormat : LocalizationFormatBase
{
    public override string FormatId => "myformat";
    public override IReadOnlyList<string> SupportedExtensions => [".myext", ".custom"];

    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using var reader = new StreamReader(stream);
        var entries = new List<LocalizationEntry>();
        
        // Your parsing logic here
        
        return new LocalizationFile(filePath ?? "unknown", entries);
    }

    public override void Write(LocalizationFile file, Stream stream)
    {
        using var writer = new StreamWriter(stream);
        
        // Your serialization logic here
    }
}

// Register in FormatRegistry
FormatRegistry.Instance.Register(new MyCustomFormat());
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

<div align="center">
  <p>Made with ❤️ by <a href="https://github.com/Taiizor">Taiizor</a></p>
  <p>
    <a href="https://github.com/Taiizor/Locale/issues">Report Bug</a> •
    <a href="https://github.com/Taiizor/Locale/issues">Request Feature</a>
  </p>
</div>