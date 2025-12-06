# Troubleshooting Guide

This guide helps you diagnose and resolve common issues when using Locale.

## Table of Contents

- [Installation Issues](#installation-issues)
- [File Detection Problems](#file-detection-problems)
- [Parsing Errors](#parsing-errors)
- [Translation Issues](#translation-issues)
- [Performance Problems](#performance-problems)
- [CLI Issues](#cli-issues)
- [Common Error Messages](#common-error-messages)

---

## Installation Issues

### .NET Tool Installation Fails

**Problem:** `dotnet tool install -g Locale.CLI` fails

**Solutions:**

1. **Update .NET SDK:**
   ```bash
   dotnet --version
   # Should be 8.0 or later
   ```

2. **Clear NuGet cache:**
   ```bash
   dotnet nuget locals all --clear
   dotnet tool install -g Locale.CLI --force
   ```

3. **Check PATH:**
   ```bash
   # Windows PowerShell
   $env:PATH -split ';' | Select-String ".dotnet"
   
   # Unix/macOS
   echo $PATH | tr ':' '\n' | grep .dotnet
   ```

4. **Manual tool path addition:**
   ```bash
   # Windows
   setx PATH "%PATH%;%USERPROFILE%\.dotnet\tools"
   
   # Unix/macOS
   export PATH="$PATH:$HOME/.dotnet/tools"
   ```

### npm Package Installation Fails

**Problem:** `npm install -g @taiizor/locale-cli` fails

**Solutions:**

1. **Check Node.js version:**
   ```bash
   node --version
   # Should be >= v18.0.0
   ```

2. **Clear npm cache:**
   ```bash
   npm cache clean --force
   npm install -g @taiizor/locale-cli
   ```

3. **Use different package manager:**
   ```bash
   # Try pnpm
   pnpm add -g @taiizor/locale-cli
   
   # Or bun
   bun add -g @taiizor/locale-cli
   ```

4. **Check npm permissions (Unix/macOS):**
   ```bash
   # Fix permissions
   sudo chown -R $(whoami) ~/.npm
   ```

### Binary Download Fails During npm Install

**Problem:** Postinstall script fails to download .NET binary

**Solutions:**

1. **Check internet connectivity:**
   ```bash
   curl -I https://github.com/Taiizor/Locale/releases/latest
   ```

2. **Manual binary download:**
   - Visit https://github.com/Taiizor/Locale/releases
   - Download appropriate binary for your platform
   - Extract to `node_modules/@taiizor/locale-cli/bin/`

3. **Configure proxy (if behind corporate firewall):**
   ```bash
   npm config set proxy http://proxy.company.com:8080
   npm config set https-proxy http://proxy.company.com:8080
   ```

---

## File Detection Problems

### Files Not Being Detected

**Problem:** `locale scan` doesn't find any files

**Diagnosis:**
```bash
# List what Locale sees
locale scan ./locales --base en --verbose

# Check file extensions
ls -R ./locales | grep -E '\.(json|yaml|resx|po|xlf|srt|vtt|csv|ftl|vb)$'
```

**Common Causes:**

1. **Wrong file extensions:**
   ```bash
   # ❌ Not detected
   messages.txt
   translations.data
   
   # ✅ Detected
   messages.json
   translations.yaml
   ```

2. **Missing culture in filename:**
   ```bash
   # ❌ Culture not detected
   messages.json
   
   # ✅ Culture detected
   messages.en.json
   en.json
   messages_en.json
   ```

3. **Files in ignored directories:**
   ```bash
   # Use --ignore to exclude
   locale scan . --base en --ignore "node_modules/**,dist/**"
   ```

### Culture Not Detected

**Problem:** Files are found but culture is not detected

**Solutions:**

1. **Use standard naming patterns:**
   ```bash
   # Recommended patterns
   en.json          # culture.ext
   messages.en.json # name.culture.ext
   en-US.json       # culture-region.ext
   messages_en.json # name_culture.ext
   ```

2. **Explicitly specify culture:**
   ```bash
   locale convert input.json output.json --culture en
   ```

3. **Check culture code validity:**
   ```csharp
   // Valid culture codes
   "en", "en-US", "tr", "de-DE", "fr-FR"
   
   // Invalid
   "english", "ENG", "en_us"
   ```

---

## Parsing Errors

### JSON Parsing Fails

**Problem:** "Invalid JSON format" error

**Diagnosis:**
```bash
# Validate JSON syntax
cat file.json | python -m json.tool

# Or use jq
jq . file.json
```

**Common Issues:**

1. **Trailing commas:**
   ```json
   {
     "key1": "value1",
     "key2": "value2",  ← Remove this comma
   }
   ```

2. **Single quotes instead of double quotes:**
   ```json
   // ❌ Wrong
   { 'key': 'value' }
   
   // ✅ Correct
   { "key": "value" }
   ```

3. **Unescaped special characters:**
   ```json
   {
     "message": "Line 1\nLine 2",     // ✅ Escaped
     "quote": "He said \"Hello\"",    // ✅ Escaped
     "path": "C:\\Users\\name"        // ✅ Escaped
   }
   ```

### YAML Parsing Fails

**Problem:** "Invalid YAML format" error

**Diagnosis:**
```bash
# Validate YAML syntax
python -c 'import yaml, sys; yaml.safe_load(sys.stdin)' < file.yaml
```

**Common Issues:**

1. **Indentation errors:**
   ```yaml
   # ❌ Wrong - inconsistent indentation
   parent:
      child1: value1
     child2: value2
   
   # ✅ Correct - consistent 2-space indentation
   parent:
     child1: value1
     child2: value2
   ```

2. **Missing space after colon:**
   ```yaml
   # ❌ Wrong
   key:value
   
   # ✅ Correct
   key: value
   ```

3. **Unquoted special values:**
   ```yaml
   # ❌ Interpreted as boolean/null
   enabled: yes
   disabled: no
   missing: null
   
   # ✅ String values
   enabled: "yes"
   disabled: "no"
   missing: "null"
   ```

### RESX Parsing Fails

**Problem:** "Invalid RESX format" error

**Solutions:**

1. **Validate XML structure:**
   ```bash
   xmllint --noout file.resx
   ```

2. **Check root element:**
   ```xml
   <!-- Must have <root> as root element -->
   <?xml version="1.0" encoding="utf-8"?>
   <root>
     <!-- content -->
   </root>
   ```

3. **Validate data entries:**
   ```xml
   <data name="Key" xml:space="preserve">
     <value>Value</value>
   </data>
   ```

### PO File Parsing Issues

**Problem:** Gettext PO files not parsing correctly

**Solutions:**

1. **Check file encoding:**
   ```bash
   # Verify UTF-8 encoding
   file -i file.po
   
   # Convert to UTF-8 if needed
   iconv -f ISO-8859-1 -t UTF-8 file.po > file_utf8.po
   ```

2. **Validate PO syntax:**
   ```bash
   msgfmt -c file.po
   ```

3. **Check msgid/msgstr format:**
   ```po
   # ✅ Correct format
   msgid "Hello"
   msgstr "Merhaba"
   
   # ✅ Multi-line strings
   msgid ""
   "Line 1\n"
   "Line 2"
   msgstr ""
   "Satır 1\n"
   "Satır 2"
   ```

---

## Translation Issues

### API Key Issues

**Problem:** "Authentication failed" error

**Solutions:**

1. **Verify API key:**
   ```bash
   # Test OpenAI key
   curl https://api.openai.com/v1/models \
     -H "Authorization: Bearer $OPENAI_API_KEY"
   
   # Test DeepL key
   curl https://api-free.deepl.com/v2/usage \
     -H "Authorization: DeepL-Auth-Key $DEEPL_API_KEY"
   ```

2. **Check environment variables:**
   ```bash
   # Windows
   echo %OPENAI_API_KEY%
   
   # Unix/macOS
   echo $OPENAI_API_KEY
   ```

3. **Use CLI option instead:**
   ```bash
   locale translate tr --from en --in ./locales \
     --provider openai --api-key "sk-..."
   ```

### Rate Limiting

**Problem:** "Rate limit exceeded" or "Too many requests" error

**Solutions:**

1. **Reduce parallelism:**
   ```bash
   locale translate tr --from en --in ./locales \
     --provider openai \
     --parallel 1 \
     --delay 1000
   ```

2. **Increase delay between calls:**
   ```bash
   # Add 2 second delay
   --delay 2000
   ```

3. **Process in batches:**
   ```bash
   # Translate one directory at a time
   for dir in app web mobile; do
     locale translate tr --from en --in ./locales/$dir \
       --provider openai --delay 1000
     sleep 5
   done
   ```

4. **Use different provider:**
   ```bash
   # Switch to DeepL or Claude
   locale translate tr --from en --in ./locales --provider deepl
   ```

### Translation Quality Issues

**Problem:** Poor translation quality or context loss

**Solutions:**

1. **Use better models:**
   ```bash
   # OpenAI - use GPT-4 instead of GPT-3.5
   --provider openai --model gpt-4
   
   # Claude - use Claude 3 Opus
   --provider claude --model claude-3-opus-20240229
   ```

2. **Add context in keys:**
   ```json
   {
     "button.save": "Save",
     "button.save.tooltip": "Save your changes",
     "menu.file.save": "Save"
   }
   ```

3. **Use comments for context:**
   ```po
   # Context: Button label for saving a document
   msgid "Save"
   msgstr ""
   ```

4. **Review and manually fix critical translations:**
   ```bash
   # Translate
   locale translate tr --from en --in ./locales
   
   # Then manually review critical strings
   ```

### Ollama Connection Issues

**Problem:** Cannot connect to Ollama

**Solutions:**

1. **Check Ollama is running:**
   ```bash
   # Start Ollama
   ollama serve
   
   # Or on macOS
   open -a Ollama
   ```

2. **Verify endpoint:**
   ```bash
   # Default endpoint
   curl http://localhost:11434/api/tags
   
   # Custom endpoint
   locale translate tr --from en --in ./locales \
     --provider ollama \
     --api-endpoint http://localhost:11434
   ```

3. **Check model is installed:**
   ```bash
   ollama list
   
   # Install model if needed
   ollama pull llama3.2
   ```

---

## Performance Problems

### Slow Scanning

**Problem:** `locale scan` takes too long

**Diagnosis:**
```bash
# Measure scan time
time locale scan ./locales --base en --targets tr,de
```

**Solutions:**

1. **Disable placeholder checking:**
   ```bash
   locale scan ./locales --base en --targets tr \
     --no-check-placeholders
   ```

2. **Scan specific directories:**
   ```bash
   # Instead of entire repo
   locale scan ./src/locales --base en --targets tr
   ```

3. **Use ignore patterns:**
   ```bash
   locale scan . --base en --targets tr \
     --ignore "node_modules/**,dist/**,build/**"
   ```

4. **Limit target cultures:**
   ```bash
   # Only scan critical languages
   locale scan ./locales --base en --targets tr,de
   ```

### High Memory Usage

**Problem:** Process uses too much memory

**Solutions:**

1. **Process files in smaller batches:**
   ```bash
   # Split into multiple scan operations
   locale scan ./locales/app --base en --targets tr
   locale scan ./locales/web --base en --targets tr
   ```

2. **Convert large files to smaller format:**
   ```bash
   # Split large JSON into multiple files
   # Or convert to more compact format
   locale convert large.json large.yaml
   ```

3. **Increase available memory:**
   ```bash
   # Increase Node.js heap size
   export NODE_OPTIONS="--max-old-space-size=4096"
   
   # Or for .NET
   export DOTNET_GCHeapHardLimit=4000000000
   ```

### Slow Translation

**Problem:** Translation takes too long

**Solutions:**

1. **Increase parallelism:**
   ```bash
   locale translate tr --from en --in ./locales \
     --parallel 10 --delay 100
   ```

2. **Use only-missing mode:**
   ```bash
   locale translate tr --from en --in ./locales \
     --only-missing
   ```

3. **Use faster model:**
   ```bash
   # OpenAI - use gpt-4o-mini instead of gpt-4
   --provider openai --model gpt-4o-mini
   
   # Gemini - use flash model
   --provider gemini --model gemini-2.0-flash
   ```

4. **Use local LLM:**
   ```bash
   # Much faster, no API delays
   locale translate tr --from en --in ./locales \
     --provider ollama --parallel 10 --delay 0
   ```

---

## CLI Issues

### Command Not Found

**Problem:** `locale: command not found`

**Solutions:**

1. **Check installation:**
   ```bash
   # .NET tool
   dotnet tool list -g | grep Locale
   
   # npm package
   npm list -g @taiizor/locale-cli
   ```

2. **Reinstall:**
   ```bash
   # .NET tool
   dotnet tool uninstall -g Locale.CLI
   dotnet tool install -g Locale.CLI
   
   # npm
   npm uninstall -g @taiizor/locale-cli
   npm install -g @taiizor/locale-cli
   ```

3. **Use npx (temporary):**
   ```bash
   npx @taiizor/locale-cli scan ./locales --base en
   ```

4. **Check PATH:**
   ```bash
   which locale
   # Should show path to binary
   ```

### Help Not Showing

**Problem:** `locale --help` shows minimal information

**Solutions:**

1. **Use command-specific help:**
   ```bash
   locale scan --help
   locale diff --help
   locale translate --help
   ```

2. **Check version:**
   ```bash
   locale --version
   ```

3. **Update to latest version:**
   ```bash
   # .NET tool
   dotnet tool update -g Locale.CLI
   
   # npm
   npm update -g @taiizor/locale-cli
   ```

### Exit Code Issues in CI/CD

**Problem:** `locale check` returns unexpected exit codes

**Understanding Exit Codes:**
```bash
0 = Success (no violations)
1 = Validation failed (when using --ci)
2 = User error (invalid arguments, missing files)
3 = Unexpected error
```

**Solutions:**

1. **Use --ci flag for CI/CD:**
   ```bash
   # Will return 1 on violations
   locale check ./locales --rules no-empty-values --ci
   ```

2. **Handle exit codes in scripts:**
   ```bash
   #!/bin/bash
   locale check ./locales --rules no-empty-values --ci
   EXIT_CODE=$?
   
   if [ $EXIT_CODE -eq 1 ]; then
     echo "Validation failed"
     exit 1
   elif [ $EXIT_CODE -eq 0 ]; then
     echo "Validation passed"
     exit 0
   else
     echo "Unexpected error"
     exit 2
   fi
   ```

3. **GitHub Actions example:**
   ```yaml
   - name: Check translations
     run: |
       locale check ./locales --rules no-empty-values,no-duplicate-keys --ci
     continue-on-error: false
   ```

---

## Common Error Messages

### "Path not found"

**Cause:** File or directory doesn't exist

**Fix:**
```bash
# Check path
ls -la ./locales

# Use absolute path
locale scan /full/path/to/locales --base en
```

### "No files found"

**Cause:** No supported files in directory or wrong extension

**Fix:**
```bash
# List supported files
find ./locales -type f \
  -name "*.json" -o -name "*.yaml" -o -name "*.resx"

# Check file extensions
locale scan ./locales --base en --verbose
```

### "Invalid culture code"

**Cause:** Culture code not recognized

**Fix:**
```bash
# Use standard codes
locale scan ./locales --base en --targets tr,de-DE,fr-FR

# Not: english, ENG, en_us
```

### "Format not supported"

**Cause:** File format not recognized

**Fix:**
```bash
# Check supported formats
locale --help

# Convert to supported format
locale convert input.txt output.json
```

### "Placeholder mismatch"

**Cause:** Different placeholders in base and target

**Fix:**
```json
{
  "message": "Hello {name}!",  // Base
  "message": "Merhaba {name}!" // Target - correct
}
```

### "Duplicate key"

**Cause:** Same key appears multiple times

**Fix:**
```json
{
  "welcome": "Welcome!",
  "welcome": "Hello!" // ❌ Remove duplicate
}
```

---

## Getting Additional Help

### Enable Verbose Mode

```bash
# Add --verbose flag for detailed output
locale scan ./locales --base en --verbose
```

### Check Logs

```bash
# .NET tool logs
ls ~/.dotnet/tools/.store/locale.cli/

# npm package logs
npm config get logs-dir
```

### Collect Diagnostic Information

```bash
# System information
uname -a  # Unix/macOS
systeminfo  # Windows

# .NET version
dotnet --info

# Node.js version
node --version
npm --version

# Locale version
locale --version

# List installed packages
dotnet tool list -g
npm list -g --depth=0
```

### Report Issues

When reporting issues, include:

1. **Locale version:** `locale --version`
2. **Operating system:** Windows/macOS/Linux + version
3. **Command used:** Full command with arguments
4. **Error message:** Complete error output
5. **Sample files:** Minimal example that reproduces the issue
6. **Expected behavior:** What you expected to happen

**GitHub Issues:** https://github.com/Taiizor/Locale/issues

**Discussions:** https://github.com/Taiizor/Locale/discussions

---

## Quick Fixes Checklist

- [ ] Restart terminal/shell after installation
- [ ] Update to latest version
- [ ] Clear caches (NuGet, npm)
- [ ] Check file paths and extensions
- [ ] Verify culture code format
- [ ] Check API keys and permissions
- [ ] Review rate limits and delays
- [ ] Enable verbose mode for details
- [ ] Test with minimal example
- [ ] Check documentation and examples

---

## Additional Resources

- [Main README](../README.md)
- [API Reference](./API-REFERENCE.md)
- [Error Handling Guidelines](./ERROR-HANDLING.md)
- [Performance Guide](./PERFORMANCE.md)
- [Examples](../examples/)
- [Contributing Guide](../.github/CONTRIBUTING.md)