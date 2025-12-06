# Changelog

All notable changes to the Locale project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Performance
- Optimized `LocalizationFile.GetValue()` and `ContainsKey()` methods to use cached dictionary instead of linear search, improving lookup performance from O(n) to O(1)

## [0.0.11] - 2025-12-06

### Added
- Multi-format localization library supporting 11 different formats (JSON, YAML, RESX, PO, XLIFF, SRT, VTT, CSV, i18next, Fluent FTL, VB)
- CLI tool with 7 commands: scan, diff, check, convert, generate, watch, translate
- Auto-translation support for 10 providers (Google, DeepL, Bing, Yandex, LibreTranslate, OpenAI, Claude, Gemini, Azure OpenAI, Ollama)
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