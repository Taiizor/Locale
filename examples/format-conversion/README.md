# Format Conversion Examples

Examples demonstrating how to convert between different localization formats.

## Common Conversion Scenarios

### 1. JSON to YAML (and vice versa)

```bash
# JSON to YAML
locale convert en.json en.yaml

# YAML to JSON
locale convert en.yaml en.json

# Batch conversion
locale convert ./json-files ./yaml-files --to yaml --recursive
```

**Programmatic Usage:**

```csharp
using Locale.Services;
using Locale.Models;

var convertService = new ConvertService();

// Single file
convertService.Convert("en.json", "en.yaml", new ConvertOptions 
{ 
    ToFormat = "yaml" 
});

// Directory
convertService.ConvertDirectory(
    "./json-locales",
    "./yaml-locales",
    new ConvertOptions { ToFormat = "yaml", Recursive = true }
);
```

### 2. RESX to PO (Gettext)

Useful when migrating from .NET RESX to cross-platform PO format.

```bash
# Single file
locale convert Resources.en.resx messages.en.po

# Batch conversion for multiple cultures
locale convert ./Resources ./po-files --to po --recursive
```

**Example: Convert .NET Resources to Gettext**

```csharp
using Locale.Services;
using Locale.Formats;

var convertService = new ConvertService();

// Convert all RESX files in a directory
string[] resxFiles = Directory.GetFiles("./Resources", "*.resx");

foreach (var resxFile in resxFiles)
{
    string poFile = Path.ChangeExtension(resxFile, ".po")
        .Replace("Resources", "po-files");
    
    convertService.Convert(resxFile, poFile, new ConvertOptions 
    { 
        ToFormat = "po" 
    });
    
    Console.WriteLine($"Converted {Path.GetFileName(resxFile)} to {Path.GetFileName(poFile)}");
}
```

### 3. XLIFF to JSON

Common when working with translation management systems (TMS).

```bash
# Convert XLIFF 1.2 to JSON
locale convert translations.xlf translations.json

# Convert XLIFF 2.0 to nested JSON
locale convert translations.xliff translations.json --format json
```

**Programmatic:**

```csharp
using Locale.Services;
using Locale.Formats;

var convertService = new ConvertService();

// XLIFF to JSON with culture detection
convertService.Convert(
    "translations.en-US.xlf",
    "en.json",
    new ConvertOptions 
    { 
        ToFormat = "json",
        Culture = "en"  // Override auto-detection
    }
);
```

### 4. i18next JSON to Standard JSON

Flatten nested i18next JSON structure to standard key-value format.

```bash
locale convert en.i18n.json en.json
```

**Before (en.i18n.json):**
```json
{
  "home": {
    "title": "Welcome",
    "subtitle": "Get started now"
  },
  "about": {
    "title": "About Us"
  }
}
```

**After (en.json):**
```json
{
  "home.title": "Welcome",
  "home.subtitle": "Get started now",
  "about.title": "About Us"
}
```

### 5. Subtitle Format Conversions

Convert between SRT and VTT subtitle formats.

```bash
# SRT to VTT
locale convert movie.en.srt movie.en.vtt

# VTT to SRT
locale convert movie.en.vtt movie.en.srt

# Batch conversion
locale convert ./srt-subs ./vtt-subs --to vtt --recursive
```

### 6. CSV to JSON

Useful for spreadsheet-based translation workflows.

```bash
# 2-column CSV to JSON
locale convert translations.csv translations.json

# Multi-language CSV (splits into separate files per language)
locale convert translations-multi.csv ./output --to json
```

**Example CSV formats:**

**2-column CSV (translations.csv):**
```csv
key,value
home.title,Welcome
home.subtitle,Get started
about.title,About Us
```

**Multi-language CSV (translations-multi.csv):**
```csv
key,en,tr,de
home.title,Welcome,Hoş geldiniz,Willkommen
home.subtitle,Get started,Başlayın,Loslegen
about.title,About Us,Hakkımızda,Über uns
```

### 7. Fluent FTL to JSON

Convert Mozilla Fluent format to standard JSON.

```bash
locale convert messages.en.ftl messages.en.json
```

**Before (messages.en.ftl):**
```fluent
welcome = Welcome to our app!
greeting = Hello, { $name }!
unread-emails = You have { $count ->
    [one] one unread email
   *[other] { $count } unread emails
}
```

**After (messages.en.json):**
```json
{
  "welcome": "Welcome to our app!",
  "greeting": "Hello, { $name }!",
  "unread-emails": "You have { $count } unread emails"
}
```

**Note:** Complex Fluent features (variants, selectors) are simplified during conversion.

## Batch Conversion Strategies

### Strategy 1: Migrate Entire Project

```bash
#!/bin/bash
# migrate-to-yaml.sh

echo "Migrating all JSON files to YAML..."

# Backup original files
cp -r ./locales ./locales.backup

# Convert all JSON files to YAML
locale convert ./locales ./locales-yaml --to yaml --recursive --force

# Verify conversion
locale diff ./locales/en.json ./locales-yaml/en.yaml

echo "Migration complete! Review ./locales-yaml/"
```

### Strategy 2: Parallel Format Support

Maintain multiple formats simultaneously during migration.

```csharp
using Locale.Services;

public class MultiFormatSync
{
    public static void SyncFormats(string sourceDir, Dictionary<string, string> targetFormats)
    {
        var convertService = new ConvertService();
        
        foreach (var (format, targetDir) in targetFormats)
        {
            Console.WriteLine($"Converting to {format}...");
            
            convertService.ConvertDirectory(
                sourceDir,
                targetDir,
                new ConvertOptions 
                { 
                    ToFormat = format,
                    Recursive = true,
                    Overwrite = true
                }
            );
        }
    }
    
    public static void Main()
    {
        var formats = new Dictionary<string, string>
        {
            ["yaml"] = "./locales-yaml",
            ["po"] = "./locales-po",
            ["xliff"] = "./locales-xliff"
        };
        
        SyncFormats("./locales-json", formats);
    }
}
```

## Format-Specific Considerations

### RESX → PO Conversion

- **Namespaced keys**: `Form1_ButtonOK` becomes `Form1.ButtonOK`
- **Comments**: RESX comments are preserved as translator comments
- **Metadata**: Resource metadata (mime-type, etc.) is lost

### JSON → YAML Conversion

- **Nested structures**: Both support nesting, structure is preserved
- **Comments**: JSON doesn't support comments, YAML comments are added
- **Formatting**: YAML is more readable for humans

### XLIFF → JSON Conversion

- **Source/Target**: Only target text is used; source is stored as comment
- **States**: Translation states (translated, needs-review) are lost
- **Context**: Context-group information is simplified

### Subtitle Formats (SRT/VTT)

- **Timing**: Preserved as comments in the key (e.g., `00:00:12,000 --> 00:00:15,000`)
- **Styling**: VTT styling tags may be simplified
- **Cue IDs**: Used as keys when available

## Validation After Conversion

Always validate conversions to ensure no data loss:

```bash
# Compare entry counts
locale scan ./original --base en --output original-report.json
locale scan ./converted --base en --output converted-report.json

# Diff the reports to ensure same number of keys
diff original-report.json converted-report.json

# Check for empty values after conversion
locale check ./converted --rules no-empty-values --ci
```

## Troubleshooting

### Issue: Keys are duplicated after conversion

**Solution:** Some formats (like CSV) may have duplicate keys. Run check first:

```bash
locale check ./source --rules no-duplicate-keys --ci
```

### Issue: Nested structure is lost

**Solution:** Use i18next format for preserving nesting:

```bash
locale convert nested.json flat.json  # Flattens
locale convert flat.json nested.i18n.json  # Preserves nesting
```

### Issue: Special characters are escaped differently

**Solution:** Verify the encoding in both formats:

```csharp
// Specify encoding explicitly
var options = new ConvertOptions 
{ 
    ToFormat = "yaml",
    Encoding = Encoding.UTF8
};
```

## Best Practices

1. **Always backup** before batch conversions
2. **Validate** using `locale check` and `locale diff`
3. **Test roundtrip** conversions (A → B → A) to ensure data fidelity
4. **Review diffs** in version control before committing
5. **Document** format choices in your project README
6. **Use CI/CD** to prevent format drift