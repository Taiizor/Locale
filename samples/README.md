# Sample Localization Files

This directory contains sample English localization files for all formats supported by Locale. These files can be used for testing and as reference implementations.

## Supported Formats

| Format | Extension(s) | Sample File | Description |
|--------|-------------|-------------|-------------|
| 📄 JSON | `.json` | `en.json` | Flat and nested JSON structures |
| 📝 YAML | `.yaml`, `.yml` | `en.yaml`, `en.yml` | Flat and nested YAML structures |
| 🔧 RESX | `.resx` | `en.resx` | .NET XML resource files |
| 📋 PO | `.po` | `en.po` | GNU Gettext translation files |
| 🔀 XLIFF | `.xlf`, `.xliff` | `en.xlf`, `en.xliff` | XML Localization Interchange (1.2) |
| 🎬 SRT | `.srt` | `en.srt` | SubRip subtitle files |
| 📺 VTT | `.vtt` | `en.vtt` | WebVTT subtitle files |
| 📊 CSV | `.csv` | `en.csv` | Comma-separated values |
| 🌍 i18next | `.i18n.json` | `en.i18n.json` | i18next-style nested JSON |
| 🦊 Fluent | `.ftl` | `en.ftl` | Mozilla Fluent FTL files |
| 🔷 VB | `.vb` | `en.vb` | Visual Basic resource wrappers (read-only) |

## Usage Examples

### Scan for Translation Gaps

```bash
# Compare English base to other target cultures
locale scan ./samples/locales --base en --targets tr,de

# Generate JSON report
locale scan ./samples/locales --base en --output report.json
```

### Convert Between Formats

```bash
# Convert JSON to YAML
locale convert ./samples/locales/en.json ./samples/locales/en-converted.yaml

# Convert RESX to PO
locale convert ./samples/locales/en.resx ./samples/locales/en-converted.po
```

### Validate Files

```bash
# Check for validation errors
locale check ./samples/locales

# Check with specific rules
locale check ./samples/locales --rules no-empty-values,consistent-placeholders
```

### Compare Two Files

```bash
# Diff two files
locale diff ./samples/locales/en.json ./samples/locales/en.yaml
```

### Generate Target Files

```bash
# Generate Turkish skeleton from English
locale generate tr --from en --in ./samples/locales --out ./samples/locales
```

## Sample Content

All sample files contain the same set of translations organized into the following categories:

- **app** - Application name and description
- **common** - Common actions (OK, Cancel, Save, Delete, Edit, Close, etc.)
- **home** - Home page content (title, subtitle, get started)
- **messages** - User messages (success, warning, confirmDelete, itemNotFound)
- **validation** - Form validation messages (required, email, minLength, maxLength)
- **navigation** - Navigation items (home, settings, help, about)

## Notes

- The **VB** format (`.vb`) is read-only and primarily used for inspection purposes
- Subtitle formats (**SRT**, **VTT**) use timing information stored as comments
- **i18next** format uses `{{count}}` placeholders for interpolation
- **Fluent** format includes examples of attributes and multiline values
