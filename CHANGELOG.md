# Changelog

All notable changes to the Locale project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `--base` / `BaseLanguage` option for projects with a non-English neutral language (e.g. .NET projects where `Resources.resx` holds German strings and `Resources.en.resx` is the English translation). Files without a culture suffix are now treated as belonging to the configured base culture across `translate`, `generate`, `scan`, `check`, and `watch` commands. When `--base` is set and `--from` is omitted, `--from` defaults to the base value. ([#24](https://github.com/Taiizor/Locale/issues/24))

### Changed
- Updated default models for AI translation providers to current 2026 generations:
  - **OpenAI**: `gpt-4o-mini` → `gpt-5.4-mini` (gpt-4o was deprecated in February 2026)
  - **Anthropic Claude**: `claude-3-5-sonnet-latest` → `claude-sonnet-4-6` (Claude 3.x family retired; new dateless pinned-snapshot format)
  - **Google Gemini**: `gemini-2.0-flash` → `gemini-2.5-flash` (gemini-2.0-flash shuts down June 1, 2026)
  - **Azure OpenAI**: `gpt-4` → `gpt-5.4-mini`
  - **Ollama**: `llama3.2` → `llama3.3` (improved multilingual support)

### Performance
- Optimized `LocalizationFile.GetValue()` and `ContainsKey()` methods to use cached dictionary instead of linear search, improving lookup performance from O(n) to O(1)

## [0.0.11] - 2025-12-06

### Added
- Multi-format localization library supporting 11 different formats (JSON, YAML, RESX, PO, XLIFF, SRT, VTT, CSV, i18next, Fluent FTL, VB)
- CLI tool with 7 commands: scan, diff, check, convert, generate, watch, translate
- Auto-translation support for 11 providers (Google, DeepL, Bing, Yandex, LibreTranslate, OpenAI, Claude, Gemini, Azure OpenAI, Ollama, Nvidia)
- Cross-platform distribution via NuGet (.NET tool) and npm package
- Comprehensive test suite with 113 tests covering all formats and services
- CI/CD pipeline with multi-platform builds (Ubuntu, Windows, macOS)
- Code coverage reporting via Codecov

### Features
- **Scan**: Compare localization files across cultures, detect missing/orphan keys and empty values
- **Diff**: Side-by-side comparison of two files with placeholder mismatch detection
- **Check**: Validate against configurable rules with CI/CD exit codes
- **Convert**: Transform between 11 different localization formats
- **Generate**: Create skeleton target files from a base language
- **Watch**: File system watcher that auto-runs scan/check on changes
- **Translate**: Auto-translate using multiple providers including AI models

[Unreleased]: https://github.com/Taiizor/Locale/compare/v0.0.11...HEAD
[0.0.11]: https://github.com/Taiizor/Locale/releases/tag/v0.0.11