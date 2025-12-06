# Frequently Asked Questions (FAQ)

Common questions and answers about Locale.

## Table of Contents

- [General](#general)
- [Installation](#installation)
- [Usage](#usage)
- [Formats](#formats)
- [Translation](#translation)
- [Performance](#performance)
- [Integration](#integration)
- [Troubleshooting](#troubleshooting)

---

## General

### What is Locale?

Locale is a comprehensive multi-format localization library and CLI tool for .NET that supports 11 different localization file formats. It provides tools for scanning, comparing, validating, converting, and auto-translating localization files.

### What makes Locale different from other localization tools?

- **Multi-format support:** Works with 11 formats including JSON, YAML, RESX, PO, XLIFF, SRT, VTT, CSV, i18next, Fluent FTL, and VB resources
- **Format conversion:** Seamlessly convert between any supported formats
- **Auto-translation:** Built-in support for 10 translation providers including AI models
- **CLI and library:** Use as a command-line tool or integrate into your .NET applications
- **Cross-platform:** Works on Windows, macOS, and Linux
- **Modern .NET:** Built with .NET 8/9/10, nullable reference types, and latest C# features

### Is Locale free?

Yes, Locale is open-source and licensed under the MIT License. It's completely free to use in both personal and commercial projects.

### Which .NET versions are supported?

Locale targets .NET 8.0, 9.0, and 10.0. You need at least .NET 8.0 SDK installed to use it.

### Can I use Locale with non-.NET projects?

Yes! The CLI tool can be installed via npm and works with any project type. You can use it with JavaScript/TypeScript, Python, Ruby, Java, or any other language that uses supported localization formats.

---

## Installation

### How do I install Locale?

**As a .NET global tool:**
```bash
dotnet tool install -g Locale.CLI
```

**Via npm/pnpm/bun/yarn:**
```bash
npm install -g @taiizor/locale-cli
```

**As a NuGet library:**
```bash
dotnet add package Locale
```

### Which installation method should I use?

- **npm:** Best for JavaScript/TypeScript projects or if you already use npm
- **.NET tool:** Best for .NET projects or if you prefer .NET tooling
- **NuGet library:** For programmatic use in .NET applications

### Can I use Locale without installing it globally?

Yes, you can use npx for temporary usage:
```bash
npx @taiizor/locale-cli scan ./locales --base en
```

Or install locally in your project:
```bash
npm install --save-dev @taiizor/locale-cli
```

### How do I update Locale?

**npm:**
```bash
npm update -g @taiizor/locale-cli
```

**.NET tool:**
```bash
dotnet tool update -g Locale.CLI
```

### How do I uninstall Locale?

**npm:**
```bash
npm uninstall -g @taiizor/locale-cli
```

**.NET tool:**
```bash
dotnet tool uninstall -g Locale.CLI
```

---

## Usage

### How do I scan my localization files?

```bash
locale scan ./locales --base en --targets tr,de,fr
```

### Can I scan without specifying target cultures?

Yes, Locale will auto-detect all cultures in your directory:
```bash
locale scan ./locales --base en
```

### How do I compare two files?

```bash
locale diff en.json tr.json
```

### How do I validate my files in CI/CD?

```bash
locale check ./locales --rules no-empty-values,no-duplicate-keys --ci
```

The `--ci` flag makes the command return exit code 1 on violations.

### How do I convert between formats?

**Single file:**
```bash
locale convert en.json en.yaml
```

**Batch conversion:**
```bash
locale convert ./json-locales ./yaml-locales --to yaml --recursive
```

### How do I generate skeleton files for a new language?

```bash
locale generate tr --from en --in ./locales --out ./locales
```

This creates Turkish translation files with all keys from English.

### Can I watch files for changes?

Yes:
```bash
locale watch ./locales --base en --mode scan
```

This will re-run the scan whenever files change.

---

## Formats

### Which formats are supported?

- **JSON** (`.json`) - Flat and nested
- **YAML** (`.yaml`, `.yml`) - Flat and nested
- **RESX** (`.resx`) - .NET XML resources
- **PO** (`.po`) - GNU Gettext
- **XLIFF** (`.xlf`, `.xliff`) - XML Localization Interchange Format (1.2 & 2.0)
- **SRT** (`.srt`) - SubRip subtitles
- **VTT** (`.vtt`) - WebVTT subtitles
- **CSV** (`.csv`) - Comma-separated values
- **i18next JSON** (`.i18n.json`) - i18next-specific format
- **Fluent FTL** (`.ftl`) - Mozilla Fluent
- **VB Resources** (`.vb`) - Visual Basic resource wrappers (read-only)

### Can I convert from any format to any other?

Yes, with few exceptions:
- VB resources are **read-only** (cannot write)
- Some format-specific features may not translate perfectly (e.g., Fluent attributes, subtitle timing)

### How does culture detection work?

Locale looks for culture codes in filenames:

**Supported patterns:**
- `en.json` - culture.ext
- `messages.en.json` - name.culture.ext
- `messages_en.json` - name_culture.ext
- `en-US.json` - culture-region.ext

### What happens to nested JSON/YAML structures?

Locale flattens nested structures using dot notation:

```json
{
  "home": {
    "title": "Welcome",
    "subtitle": "Hello there"
  }
}
```

Becomes:
```
home.title = Welcome
home.subtitle = Hello there
```

### Can I preserve nested structure when converting?

Currently, Locale uses flat key-value pairs internally. Nested JSON/YAML is flattened during parsing and can be reconstructed when writing to JSON/YAML formats.

### Do you support pluralization?

Basic support:
- **PO files:** Reads `msgid_plural` and `msgstr[n]`
- **Fluent:** Reads Fluent's plural forms
- **Other formats:** Store plural forms as separate keys (e.g., `items_one`, `items_other`)

### What about context and comments?

- **Supported:** PO (`msgctxt`), RESX (`<comment>`), XLIFF (`<note>`)
- **Preserved:** Comments are maintained during roundtrip conversions where format supports them
- **CLI:** Use `--preserve-comments` flag (where applicable)

---

## Translation

### Which translation providers are supported?

- **Google Translate** (free, no API key)
- **DeepL** (API key required)
- **Bing Translator** (API key required)
- **Yandex Translate** (API key required)
- **LibreTranslate** (optional API key, self-hostable)
- **OpenAI ChatGPT** (API key required)
- **Anthropic Claude** (API key required)
- **Google Gemini** (API key required)
- **Azure OpenAI** (API key required)
- **Ollama** (local LLM, no API key)

### How do I get API keys?

- **OpenAI:** https://platform.openai.com/api-keys
- **DeepL:** https://www.deepl.com/pro-api
- **Claude:** https://console.anthropic.com/
- **Gemini:** https://makersuite.google.com/app/apikey
- **Bing:** https://azure.microsoft.com/services/cognitive-services/translator/
- **Yandex:** https://tech.yandex.com/translate/

### Which provider should I use?

**For free translations:**
- **Google Translate:** Good general-purpose option

**For quality:**
- **DeepL:** Best for European languages
- **OpenAI GPT-4:** Best context-awareness
- **Claude 3 Opus:** Best for nuanced translations

**For speed:**
- **Gemini Flash:** Fast AI model
- **OpenAI GPT-4o-mini:** Fast and cost-effective
- **Ollama:** Fastest (local, no network latency)

**For privacy:**
- **Ollama:** Completely local
- **LibreTranslate:** Self-hosted option

### How much does translation cost?

**Free:**
- Google Translate (unofficial API)
- Ollama (local LLM)

**Paid (typical costs per 1M characters):**
- OpenAI GPT-4o-mini: ~$0.15
- OpenAI GPT-4: ~$30
- DeepL: ~$20
- Claude: ~$3-15 (depending on model)
- Gemini: ~$0.50-7 (depending on model)

### How do I translate only missing keys?

```bash
locale translate tr --from en --in ./locales --only-missing
```

This is the default behavior. Use `--overwrite-existing` to retranslate everything.

### Can I translate multiple languages at once?

Yes, run multiple commands:
```bash
locale translate tr --from en --in ./locales --provider google
locale translate de --from en --in ./locales --provider google
locale translate fr --from en --in ./locales --provider google
```

Or use a script:
```bash
for lang in tr de fr es it; do
  locale translate $lang --from en --in ./locales --provider google
done
```

### How do I speed up translation?

1. **Increase parallelism:**
   ```bash
   --parallel 10
   ```

2. **Use faster model:**
   ```bash
   --provider openai --model gpt-4o-mini
   ```

3. **Use local LLM:**
   ```bash
   --provider ollama
   ```

4. **Translate only missing:**
   ```bash
   --only-missing
   ```

### How do I handle rate limits?

1. **Reduce parallelism:**
   ```bash
   --parallel 1
   ```

2. **Increase delay:**
   ```bash
   --delay 1000  # 1 second between calls
   ```

3. **Process in batches:**
   ```bash
   # Translate one directory at a time with pauses
   ```

### Can I use my own translation API?

Currently, Locale supports the listed providers. To add custom providers:
1. Fork the repository
2. Implement a new provider class
3. Submit a pull request

Or use Locale programmatically and implement translation yourself.

---

## Performance

### How fast is Locale?

**Parsing performance** (typical):
- Small files (< 1000 keys): < 10ms
- Medium files (1000-10,000 keys): 10-100ms
- Large files (> 10,000 keys): 100ms-1s

**Scanning performance:**
- 100 files: ~2-5 seconds
- 1000 files: ~15-30 seconds (sequential)
- 1000 files: ~5-10 seconds (parallel)

**Translation performance:**
- Google Translate: ~1 key/second
- OpenAI (parallel): ~5-10 keys/second
- Ollama (local): ~20-50 keys/second

### How can I improve performance?

See the [Performance Guide](./PERFORMANCE.md) for detailed optimization strategies.

**Quick tips:**
- Use `--parallel` for translation
- Disable `--check-placeholders` if not needed
- Use `--ignore` patterns to exclude unnecessary directories
- Process in batches for very large repositories

### Does Locale cache results?

Yes, internally:
- `LocalizationFile` caches key lookups
- Format detection is cached per file extension

But Locale doesn't persist caches between runs.

### Can I process files in parallel?

When using Locale as a library, yes:
```csharp
Parallel.ForEach(files, file =>
{
    var service = new ScanService();
    var report = service.Scan(file, options);
});
```

CLI commands process files efficiently but don't expose parallel options (except for translation).

---

## Integration

### How do I integrate Locale into CI/CD?

**GitHub Actions:**
```yaml
- name: Check translations
  run: |
    npm install -g @taiizor/locale-cli
    locale check ./locales --rules no-empty-values --ci
```

**GitLab CI:**
```yaml
check-translations:
  script:
    - npm install -g @taiizor/locale-cli
    - locale check ./locales --rules no-empty-values --ci
```

**Azure Pipelines:**
```yaml
- script: |
    npm install -g @taiizor/locale-cli
    locale check ./locales --rules no-empty-values --ci
  displayName: 'Check translations'
```

### Can I use Locale with pre-commit hooks?

Yes! Create `.git/hooks/pre-commit`:
```bash
#!/bin/bash
locale check ./locales --rules no-empty-values,no-duplicate-keys --ci
```

Or use Husky (for npm projects):
```json
{
  "husky": {
    "hooks": {
      "pre-commit": "locale check ./locales --rules no-empty-values --ci"
    }
  }
}
```

### How do I generate reports for dashboards?

Use JSON output:
```bash
locale scan ./locales --base en --targets tr,de --output report.json
```

Then parse `report.json` in your dashboard tool.

### Can I use Locale in my .NET application?

Yes! Install the NuGet package:
```bash
dotnet add package Locale
```

Then use the services:
```csharp
using Locale.Services;

var service = new ScanService();
var report = service.Scan("./locales", new ScanOptions
{
    BaseCulture = "en",
    TargetCultures = ["tr", "de"]
});
```

See the [API Reference](./API-REFERENCE.md) for details.

### Does Locale work with monorepos?

Yes! You can:
- Run Locale in each sub-project
- Scan the entire monorepo at once
- Use `--ignore` patterns to exclude specific directories

```bash
locale scan . --base en --ignore "node_modules/**,dist/**,build/**"
```

---

## Troubleshooting

### Locale command not found

**Solutions:**
1. Restart your terminal
2. Check PATH contains `.dotnet/tools` (for .NET tool)
3. Reinstall: `dotnet tool install -g Locale.CLI --force`

See [Troubleshooting Guide](./TROUBLESHOOTING.md#command-not-found) for details.

### Files not being detected

**Common causes:**
- Wrong file extension
- Culture not in filename
- Files in ignored directories

**Solution:**
```bash
# Use verbose mode
locale scan ./locales --base en --verbose
```

### Translation fails with authentication error

**Solution:**
1. Verify API key is correct
2. Check environment variables
3. Use `--api-key` option directly

```bash
locale translate tr --from en --in ./locales \
  --provider openai --api-key "sk-..."
```

### Rate limit exceeded

**Solution:**
```bash
locale translate tr --from en --in ./locales \
  --parallel 1 --delay 1000
```

### "Invalid JSON format" error

**Solution:**
1. Validate JSON: `cat file.json | python -m json.tool`
2. Remove trailing commas
3. Use double quotes, not single quotes

See [Troubleshooting Guide](./TROUBLESHOOTING.md) for more solutions.

---

## Community and Support

### Where can I get help?

- **Documentation:** https://github.com/Taiizor/Locale
- **Issues:** https://github.com/Taiizor/Locale/issues
- **Discussions:** https://github.com/Taiizor/Locale/discussions

### How do I report a bug?

1. Check if it's already reported: https://github.com/Taiizor/Locale/issues
2. Create a new issue with:
   - Locale version
   - Operating system
   - Command used
   - Error message
   - Minimal example to reproduce

### How do I request a feature?

Open a discussion or issue on GitHub:
https://github.com/Taiizor/Locale/issues/new

### Can I contribute?

Yes! Contributions are welcome. See the [Contributing Guide](../.github/CONTRIBUTING.md).

**Ways to contribute:**
- Report bugs
- Request features
- Improve documentation
- Add new formats
- Add new translation providers
- Fix issues

### Who maintains Locale?

Locale is maintained by [Taiizor](https://github.com/Taiizor) and the open-source community.

---

## Advanced Topics

### Can I create custom format handlers?

Yes! Implement `ILocalizationFormat` or extend `LocalizationFormatBase`:

```csharp
public sealed class MyCustomFormat : LocalizationFormatBase
{
    public override string FormatId => "myformat";
    public override IReadOnlyList<string> SupportedExtensions => [".myext"];
    
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        // Your parsing logic
    }
    
    public override void Write(LocalizationFile file, Stream stream)
    {
        // Your writing logic
    }
}

// Register
FormatRegistry.Default.Register(new MyCustomFormat());
```

See [examples/custom-format](../examples/custom-format) for a complete example.

### How do I add custom validation rules?

Currently, rules are built-in. To add custom rules:
1. Fork the repository
2. Add your rule to `CheckService`
3. Submit a pull request

Or use Locale programmatically and implement validation yourself.

### Can I use Locale with dependency injection?

Yes:
```csharp
services.AddSingleton<FormatRegistry>(FormatRegistry.Default);
services.AddScoped<ScanService>();
services.AddScoped<ConvertService>();
services.AddScoped<TranslateService>();
```

### How do I extend translation providers?

Currently not exposed as a plugin system. To add providers:
1. Fork the repository
2. Add provider implementation to `TranslateService`
3. Submit a pull request

---

## Didn't find your answer?

- Check the [Troubleshooting Guide](./TROUBLESHOOTING.md)
- Read the [API Reference](./API-REFERENCE.md)
- Browse the [Examples](../examples/)
- Ask in [Discussions](https://github.com/Taiizor/Locale/discussions)
- Open an [Issue](https://github.com/Taiizor/Locale/issues)