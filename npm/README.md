# 🌍 Locale CLI

<div align="center">
  <p><strong>Multi-format localization CLI tool for npm / pnpm / bun / yarn</strong></p>
  <p>Scan, diff, validate, convert, generate, watch, and auto-translate translation files across 11 formats</p>

  <p>
    <a href="https://www.npmjs.com/package/@taiizor/locale-cli"><img src="https://img.shields.io/npm/v/@taiizor/locale-cli?style=flat-square&logo=npm" alt="npm Version"></a>
    <a href="https://www.npmjs.com/package/@taiizor/locale-cli"><img src="https://img.shields.io/npm/dm/@taiizor/locale-cli?style=flat-square&logo=npm" alt="npm Downloads"></a>
    <a href="https://github.com/Taiizor/Locale/blob/develop/LICENSE"><img src="https://img.shields.io/github/license/Taiizor/Locale?style=flat-square" alt="License"></a>
  </p>

  <p>
    <a href="#-installation">Installation</a> •
    <a href="#-commands">Commands</a> •
    <a href="#-supported-formats">Formats</a> •
    <a href="#-translation-providers">Providers</a> •
    <a href="#-troubleshooting">Troubleshooting</a>
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

```bash
# npm
npm install -g @taiizor/locale-cli

# pnpm
pnpm add -g @taiizor/locale-cli

# bun (you may need to trust the package for postinstall)
bun add -g @taiizor/locale-cli
# If the binary isn't downloaded, run: bun pm trust @taiizor/locale-cli && bun install

# yarn
yarn global add @taiizor/locale-cli
```

> **💡 Note**: If the postinstall script doesn't run (common with bun or when using `--ignore-scripts`), the CLI will automatically attempt to download the binary on first run.

## 🚀 Quick Start

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

## 📋 Commands

| Command | Description |
|---------|-------------|
| `locale scan` | Compare localization files across cultures, detect missing/orphan keys |
| `locale diff` | Side-by-side comparison of two files |
| `locale check` | Validate against configurable rules with CI/CD exit codes |
| `locale convert` | Transform between 11 different localization formats |
| `locale generate` | Create skeleton target files from a base language |
| `locale watch` | File system watcher that auto-runs scan/check on changes |
| `locale translate` | Auto-translate using 10 providers including AI |

### ✅ Validation Rules

```bash
locale check ./locales --rules no-empty-values,consistent-placeholders --ci
```

| Rule | Description |
|------|-------------|
| `no-empty-values` | Flag keys with empty or whitespace-only values |
| `no-duplicate-keys` | Flag duplicate keys in a file |
| `no-orphan-keys` | Flag keys that exist in target but not in base |
| `consistent-placeholders` | Ensure placeholders match between cultures |
| `no-trailing-whitespace` | Flag values with trailing whitespace |

### ⚡ Parallel Translation Options

```bash
# Parallel translation (5 concurrent requests with 500ms delay)
locale translate tr --from en --in ./locales --parallel 5 --delay 500
```

| Option | Default | Description |
|--------|---------|-------------|
| `--parallel` | `1` | Degree of parallelism (1 = sequential, higher = faster) |
| `--delay` | `100` | Delay between API calls in milliseconds (for rate limiting) |

**💡 Tips:**
- Use `--parallel 5` or higher for faster translations on large files
- Increase `--delay` if you hit API rate limits
- Sequential mode (`--parallel 1`) is safest for strict rate-limited APIs

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

## 🔧 Alternative Installation

If you have .NET SDK installed, you can also use the .NET global tool:

```bash
dotnet tool install -g Locale.CLI
```

## 🛠️ Troubleshooting

### Binary not found after installation

If you see "Locale CLI binary not found" error:

<details>
<summary><strong>🔸 For bun users</strong></summary>

Trust the package and reinstall:

```bash
bun pm trust @taiizor/locale-cli
bun install
```

</details>

<details>
<summary><strong>🔸 For npm/pnpm users</strong></summary>

Try reinstalling without ignore-scripts:

```bash
npm install -g @taiizor/locale-cli
```

</details>

<details>
<summary><strong>🔸 Alternative: Use .NET CLI</strong></summary>

If you have .NET SDK installed:

```bash
dotnet tool install -g Locale.CLI
```

</details>

<details>
<summary><strong>🔸 Manual download</strong></summary>

Download the binary for your platform from [GitHub Releases](https://github.com/Taiizor/Locale/releases) and extract it to the package's `bin/<platform>` directory.

**Supported platforms:**
- `linux-x64` - Linux (x64)
- `linux-arm64` - Linux (ARM64)
- `osx-x64` - macOS (Intel)
- `osx-arm64` - macOS (Apple Silicon)
- `win-x64` - Windows (x64)
- `win-arm64` - Windows (ARM64)

</details>

## 📖 Documentation

For full documentation and library usage, visit the [GitHub repository](https://github.com/Taiizor/Locale).

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](https://github.com/Taiizor/Locale/blob/develop/LICENSE) file for details.

---

<div align="center">
  <p>Made with ❤️ by <a href="https://github.com/Taiizor">Taiizor</a></p>
  <p>
    <a href="https://github.com/Taiizor/Locale/issues">Report Bug</a> •
    <a href="https://github.com/Taiizor/Locale/issues">Request Feature</a>
  </p>
</div>