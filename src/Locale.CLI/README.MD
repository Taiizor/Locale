# 🖥️ Locale.CLI

**Cross-platform CLI tool for localization management**

[![NuGet Version](https://img.shields.io/nuget/v/Locale.CLI?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Locale.CLI)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Locale.CLI?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Locale.CLI)
[![License](https://img.shields.io/github/license/Taiizor/Locale?style=flat-square)](https://github.com/Taiizor/Locale/blob/develop/LICENSE)

---

**Locale.CLI** is a powerful command-line tool for managing localization files. Scan, diff, validate, convert, generate, watch, and auto-translate across 11 formats with support for 10 translation providers including AI (ChatGPT, Claude, Gemini).

## 📦 Installation

```bash
dotnet tool install -g Locale.CLI
```

## 🚀 Quick Start

```bash
# Scan for translation gaps
locale scan ./locales --base en --targets tr,de,fr

# Compare two files
locale diff en.json tr.json

# Validate with CI exit codes
locale check ./locales --ci

# Convert between formats
locale convert en.json en.yaml

# Generate skeleton files
locale generate tr --from en --in ./locales --out ./locales

# Watch for changes
locale watch ./locales --base en --mode scan

# Auto-translate with AI
locale translate tr --from en --in ./locales --provider openai --api-key YOUR_KEY
```

## 📋 Commands

| Command | Description |
|---------|-------------|
| `scan` | Compare localization files across cultures |
| `diff` | Compare two localization files |
| `check` | Validate against configurable rules |
| `convert` | Transform between formats |
| `generate` | Create skeleton target files |
| `watch` | Monitor files for changes |
| `translate` | Auto-translate using external APIs |

## ✅ Validation Rules

```bash
locale check ./locales --rules no-empty-values,consistent-placeholders --ci
```

| Rule | Description |
|------|-------------|
| `no-empty-values` | Flag empty or whitespace-only values |
| `no-duplicate-keys` | Flag duplicate keys |
| `no-orphan-keys` | Flag keys missing from base culture |
| `consistent-placeholders` | Ensure placeholders match |
| `no-trailing-whitespace` | Flag trailing whitespace |

## 🌐 Translation Providers

```bash
# Free providers
locale translate tr --from en --in ./locales --provider google
locale translate tr --from en --in ./locales --provider ollama --model llama3.3

# API providers
locale translate tr --from en --in ./locales --provider deepl --api-key KEY
locale translate tr --from en --in ./locales --provider openai --api-key KEY
locale translate tr --from en --in ./locales --provider claude --api-key KEY
locale translate tr --from en --in ./locales --provider gemini --api-key KEY

# Custom JSON parameters for AI providers
locale translate tr --from en --in ./locales --provider openai --custom-params "{'temperature': 0.8, 'max_tokens': 2000}"
```

| Provider | API Key | Default Model |
|----------|---------|---------------|
| Google | ❌ | - |
| DeepL | ✅ | - |
| Bing | ✅ | - |
| Yandex | ✅ | - |
| LibreTranslate | ⚪ | - |
| OpenAI | ✅ | gpt-5.4-mini |
| Claude | ✅ | claude-sonnet-4-6 |
| Gemini | ✅ | gemini-2.5-flash |
| Azure OpenAI | ✅ | - |
| Ollama | ❌ | llama3.3 |
| Nvidia | ✅ | meta/llama-3.3-70b-instruct |

## 📁 Supported Formats

JSON, YAML, RESX, PO/Gettext, XLIFF (1.2 & 2.0), SRT, VTT, CSV, i18next JSON, Fluent FTL, and VB resources.

## 📖 Documentation

For full documentation and examples, visit the [GitHub repository](https://github.com/Taiizor/Locale).

## 📄 License

MIT License - see [LICENSE](https://github.com/Taiizor/Locale/blob/develop/LICENSE) for details.