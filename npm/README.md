# Locale CLI

<div align="center">
  <p><strong>Multi-format localization CLI tool for Node.js/npm/bun/pnpm</strong></p>
  <p>Scan, diff, validate, convert, generate, watch, and auto-translate translation files across 11 formats</p>
</div>

## Installation

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

## Usage

```bash
# Scan for translation gaps
locale scan ./locales --base en --targets tr,de,fr

# Compare two files
locale diff en.json tr.json

# Validate files
locale check ./locales --rules no-empty-values,consistent-placeholders --ci

# Convert between formats
locale convert en.json en.yaml

# Generate skeleton files
locale generate tr --from en --in ./locales --out ./locales

# Watch for changes
locale watch ./locales --base en --mode scan

# Auto-translate (Google, free)
locale translate tr --from en --in ./locales --provider google

# Auto-translate (OpenAI)
locale translate tr --from en --in ./locales --provider openai --api-key YOUR_KEY
```

## Commands

| Command | Description |
|---------|-------------|
| `scan` | Compare localization files across cultures, detect missing/orphan keys |
| `diff` | Side-by-side comparison of two files |
| `check` | Validate against configurable rules with CI/CD exit codes |
| `convert` | Transform between 11 different localization formats |
| `generate` | Create skeleton target files from a base language |
| `watch` | File system watcher that auto-runs scan/check on changes |
| `translate` | Auto-translate using 10 providers including AI |

## Supported Formats

| Format | Extensions | Description |
|--------|-----------|-------------|
| JSON | `.json` | Flat and nested JSON structures |
| YAML | `.yaml`, `.yml` | Flat and nested YAML structures |
| RESX | `.resx` | .NET XML resource files |
| PO | `.po` | GNU Gettext translation files |
| XLIFF | `.xlf`, `.xliff` | XML Localization Interchange (1.2 & 2.0) |
| SRT | `.srt` | SubRip subtitle files |
| VTT | `.vtt` | WebVTT subtitle files |
| CSV | `.csv` | Comma-separated values |
| i18next | `.i18n.json` | i18next-style nested JSON |
| Fluent | `.ftl` | Mozilla Fluent FTL files |
| VB | `.vb` | Visual Basic resource wrappers (read-only) |

## Translation Providers

| Provider | API Key | Description |
|----------|---------|-------------|
| Google | ❌ No | Quick, free translations |
| DeepL | ✅ Yes | High-quality European languages |
| Bing | ✅ Yes | Microsoft ecosystem |
| Yandex | ✅ Yes | Slavic languages |
| LibreTranslate | ⚪ Optional | Self-hosted, privacy |
| OpenAI | ✅ Yes | Context-aware AI translation |
| Claude | ✅ Yes | Nuanced translations |
| Gemini | ✅ Yes | Fast AI translations |
| Azure OpenAI | ✅ Yes | Enterprise deployments |
| Ollama | ❌ No | Local, private LLM |

## Alternative Installation

If you have .NET SDK installed, you can also use:

```bash
dotnet tool install -g Locale.CLI
```

## Documentation

For full documentation, visit: https://github.com/Taiizor/Locale

## License

MIT License - see [LICENSE](https://github.com/Taiizor/Locale/blob/develop/LICENSE)

## Author

[Taiizor](https://github.com/Taiizor)