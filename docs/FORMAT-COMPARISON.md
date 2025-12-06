# Format Comparison Matrix

Comprehensive comparison of all supported localization formats in Locale.

## Quick Reference

| Format | Read | Write | Nested | Comments | Context | Plurals | Variables | Best For |
|--------|------|-------|--------|----------|---------|---------|-----------|----------|
| **JSON** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | Custom | Web apps, APIs |
| **YAML** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | Custom | Config files, docs |
| **RESX** | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | Custom | .NET apps, WinForms |
| **PO** | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | `%s` | Linux, Python, PHP |
| **XLIFF** | ✅ | ✅ | ❌ | ✅ | ✅ | ❌ | Custom | Translation workflows |
| **SRT** | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | N/A | Video subtitles |
| **VTT** | ✅ | ✅ | ❌ | ✅ | ✅ | ❌ | N/A | Web video subtitles |
| **CSV** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | Custom | Spreadsheets, bulk |
| **i18next** | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | `{{}}` | React, Node.js apps |
| **Fluent** | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | `{}` | Firefox, Mozilla apps |
| **VB** | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | Custom | VB.NET (read-only) |

---

## Detailed Comparison

### JSON (JavaScript Object Notation)

**Extensions:** `.json`

**Pros:**
- Universal format, widely supported
- Excellent tooling and editor support
- Human-readable and easy to edit
- Supports nested structures
- Native web format

**Cons:**
- No comments support (unless using JSON5)
- No built-in pluralization
- No context information
- Trailing commas cause errors

**Structure:**
```json
{
  "welcome": "Welcome!",
  "user": {
    "greeting": "Hello, {name}!",
    "logout": "Logout"
  }
}
```

**Flattened (how Locale processes):**
```
welcome = Welcome!
user.greeting = Hello, {name}!
user.logout = Logout
```

**Best For:**
- Web applications
- REST APIs
- JavaScript/TypeScript projects
- Simple key-value translations

**Use Cases:**
- React apps with i18n
- Vue.js apps with vue-i18n
- Angular apps with ngx-translate
- Node.js backends

---

### YAML (YAML Ain't Markup Language)

**Extensions:** `.yaml`, `.yml`

**Pros:**
- More readable than JSON
- Supports comments
- Supports nested structures
- Less verbose (no quotes, braces)
- Good for configuration

**Cons:**
- Indentation-sensitive (can cause errors)
- Less universal than JSON
- Slower parsing than JSON
- No built-in pluralization

**Structure:**
```yaml
# Welcome messages
welcome: Welcome!
user:
  greeting: Hello, {name}!  # Personalized greeting
  logout: Logout
```

**Best For:**
- Configuration files
- Documentation projects
- Projects preferring readability
- CI/CD configurations

**Use Cases:**
- Jekyll/Hugo static sites
- Kubernetes configurations
- Ansible playbooks
- Documentation platforms

---

### RESX (Resource XML)

**Extensions:** `.resx`

**Pros:**
- Native .NET format
- Strong Visual Studio integration
- Supports comments
- Type-safe access in C#
- Compile-time checking

**Cons:**
- Verbose XML format
- .NET-specific
- No nested structures
- Large file size

**Structure:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Welcome" xml:space="preserve">
    <value>Welcome!</value>
    <comment>Main welcome message</comment>
  </data>
  <data name="User_Greeting" xml:space="preserve">
    <value>Hello, {0}!</value>
  </data>
</root>
```

**Best For:**
- .NET applications
- WinForms/WPF applications
- ASP.NET projects
- Strongly-typed resources

**Use Cases:**
- Desktop .NET apps
- ASP.NET MVC/Razor pages
- Blazor applications
- .NET MAUI apps

---

### PO (Gettext / Portable Object)

**Extensions:** `.po`

**Pros:**
- Industry standard for translation
- Excellent translation tool support
- Comments and context support
- Plural forms support
- Widely used in open source

**Cons:**
- Complex syntax for beginners
- Text-based (can be hard to parse)
- Legacy format
- Not JSON-friendly

**Structure:**
```po
# Main welcome message
msgid "welcome"
msgstr "Welcome!"

# Personalized greeting
#: user.greeting
msgctxt "user context"
msgid "Hello, %s!"
msgstr "Merhaba, %s!"

# Plural forms
msgid "You have %d message"
msgid_plural "You have %d messages"
msgstr[0] "Bir mesajınız var"
msgstr[1] "%d mesajınız var"
```

**Best For:**
- Linux applications
- Python projects (with gettext)
- PHP projects
- Open-source projects

**Use Cases:**
- WordPress themes/plugins
- Django applications
- GNU/Linux software
- Translation workflows with Poedit

---

### XLIFF (XML Localization Interchange File Format)

**Extensions:** `.xlf`, `.xliff`

**Pros:**
- Translation industry standard
- Rich metadata support
- Source and target in one file
- Translation memory integration
- Version 1.2 and 2.0 support

**Cons:**
- Very verbose XML
- Complex structure
- Overkill for simple projects
- Large file sizes

**Structure:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xliff version="2.0" xmlns="urn:oasis:names:tc:xliff:document:2.0" srcLang="en" trgLang="tr">
  <file id="f1">
    <unit id="welcome">
      <segment>
        <source>Welcome!</source>
        <target>Hoşgeldiniz!</target>
      </segment>
    </unit>
    <unit id="user.greeting">
      <notes>
        <note>Personalized greeting message</note>
      </notes>
      <segment>
        <source>Hello, {name}!</source>
        <target>Merhaba, {name}!</target>
      </segment>
    </unit>
  </file>
</xliff>
```

**Best For:**
- Professional translation workflows
- Translation memory systems
- CAT (Computer-Assisted Translation) tools
- Enterprise localization

**Use Cases:**
- SDL Trados projects
- MemoQ projects
- Crowdin exports
- Transifex integration

---

### SRT (SubRip)

**Extensions:** `.srt`

**Pros:**
- Simple text format
- Universal subtitle support
- Easy to edit manually
- Small file size
- Timing information included

**Cons:**
- Limited formatting
- No metadata
- Sequential numbering required
- Not ideal for general localization

**Structure:**
```srt
1
00:00:01,000 --> 00:00:04,000
Welcome to our application!

2
00:00:05,500 --> 00:00:08,000
This is a subtitle example.
```

**Best For:**
- Video subtitles
- Educational content
- Video streaming platforms
- Accessibility features

**Use Cases:**
- YouTube videos
- Educational platforms
- Video players
- Streaming services

---

### VTT (WebVTT)

**Extensions:** `.vtt`

**Pros:**
- Web standard for subtitles
- Richer formatting than SRT
- Cue styling support
- Metadata support
- Browser native support

**Cons:**
- More complex than SRT
- Not as universally supported
- Overkill for simple subtitles

**Structure:**
```vtt
WEBVTT

NOTE This is a comment

welcome
00:00:01.000 --> 00:00:04.000
Welcome to our application!

greeting
00:00:05.500 --> 00:00:08.000 align:start line:0%
This is a <b>styled</b> subtitle example.
```

**Best For:**
- Web video subtitles
- HTML5 video players
- Modern web applications
- Accessible web content

**Use Cases:**
- HTML5 `<video>` elements
- Modern web players
- Accessibility requirements
- Rich subtitle formatting

---

### CSV (Comma-Separated Values)

**Extensions:** `.csv`

**Pros:**
- Universal format
- Excel/Google Sheets compatible
- Easy bulk editing
- Simple structure
- Multi-language in one file

**Cons:**
- No nesting support
- No comments
- Escaping issues with commas/quotes
- Limited metadata

**Structure (2-column):**
```csv
key,value
welcome,Welcome!
user.greeting,"Hello, {name}!"
user.logout,Logout
```

**Structure (multi-language):**
```csv
key,en,tr,de
welcome,Welcome!,Hoşgeldiniz!,Willkommen!
user.greeting,"Hello, {name}!","Merhaba, {name}!","Hallo, {name}!"
```

**Best For:**
- Spreadsheet workflows
- Bulk editing
- Non-technical translators
- Multi-language exports

**Use Cases:**
- Translation by spreadsheet
- Bulk import/export
- Google Sheets collaboration
- Non-technical team members

---

### i18next JSON

**Extensions:** `.i18n.json`

**Pros:**
- i18next ecosystem integration
- Nested structures
- Plural forms support
- Interpolation support
- Namespace support

**Cons:**
- i18next-specific syntax
- Less universal than plain JSON
- Learning curve for i18next features

**Structure:**
```json
{
  "welcome": "Welcome!",
  "user": {
    "greeting": "Hello, {{name}}!",
    "logout": "Logout"
  },
  "items": {
    "one": "You have one item",
    "other": "You have {{count}} items"
  }
}
```

**Best For:**
- React applications with react-i18next
- Node.js with i18next
- Vue.js with vue-i18next
- i18next ecosystem

**Use Cases:**
- React apps
- Next.js applications
- Node.js APIs
- Vue.js projects

---

### Fluent FTL (Project Fluent)

**Extensions:** `.ftl`

**Pros:**
- Powerful syntax for complex translations
- Natural language support
- Gender and plural support
- Attributes for metadata
- Designed for modern localization

**Cons:**
- Less common format
- Mozilla-specific ecosystem
- Steeper learning curve
- Limited tooling outside Mozilla

**Structure:**
```ftl
# Simple message
welcome = Welcome!

# Message with variable
user-greeting = Hello, { $name }!

# Message with plural
items =
    { $count ->
        [one] You have one item
       *[other] You have { $count } items
    }

# Message with attribute
button-save = Save
    .tooltip = Save your changes
    .aria-label = Save button
```

**Best For:**
- Firefox extensions
- Mozilla projects
- Complex grammatical rules
- Natural language translations

**Use Cases:**
- Firefox add-ons
- Mozilla applications
- Complex linguistic requirements
- Projects needing grammatical correctness

---

### VB Resources

**Extensions:** `.vb`

**Pros:**
- Strongly-typed resource access
- Compile-time checking
- Visual Studio integration
- Designer support

**Cons:**
- **Read-only in Locale**
- VB.NET specific
- Generated code (not meant for editing)
- Better to use underlying `.resx`

**Structure:**
```vb
Friend Module Resources
    Friend ReadOnly Property Welcome() As String
        Get
            Return ResourceManager.GetString("Welcome", resourceCulture)
        End Get
    End Property
    
    Friend ReadOnly Property UserGreeting() As String
        Get
            Return ResourceManager.GetString("UserGreeting", resourceCulture)
        End Get
    End Property
End Module
```

**Best For:**
- Legacy VB.NET projects (read-only)
- Inspection/auditing
- Migration to other formats

**Note:** Prefer using `.resx` files directly rather than `.vb` wrappers.

---

## Format Selection Guide

### Choose JSON when:
- ✅ Building web applications
- ✅ Need universal format support
- ✅ Want simple key-value structure
- ✅ Have nested content
- ❌ Don't need comments
- ❌ Don't need pluralization

### Choose YAML when:
- ✅ Prefer readability
- ✅ Need comments
- ✅ Working with configuration
- ✅ Have nested content
- ❌ OK with indentation sensitivity

### Choose RESX when:
- ✅ Building .NET applications
- ✅ Need Visual Studio integration
- ✅ Want type safety
- ✅ Need compile-time checking

### Choose PO when:
- ✅ Need professional translation tools
- ✅ Working on Linux/open-source
- ✅ Need plural forms
- ✅ Need context support
- ✅ Using Poedit or similar

### Choose XLIFF when:
- ✅ Professional translation workflow
- ✅ Using CAT tools
- ✅ Need translation memory
- ✅ Enterprise localization

### Choose SRT/VTT when:
- ✅ Subtitle files for videos
- ✅ Accessibility requirements
- ✅ Educational content

### Choose CSV when:
- ✅ Bulk editing in spreadsheets
- ✅ Non-technical translators
- ✅ Simple structure
- ✅ Multi-language exports

### Choose i18next when:
- ✅ Using i18next library
- ✅ React/Vue.js applications
- ✅ Need plural forms
- ✅ Need interpolation

### Choose Fluent when:
- ✅ Firefox extensions
- ✅ Complex grammatical rules
- ✅ Natural language requirements
- ✅ Mozilla ecosystem

---

## Conversion Considerations

### Converting from JSON/YAML to RESX
- ✅ Preserves keys and values
- ⚠️ Nested structures become flat (e.g., `user.greeting`)
- ❌ No comments in JSON (preserved in YAML)

### Converting from RESX to JSON/YAML
- ✅ Preserves keys and values
- ✅ Comments become JSON comments (if supported)
- ⚠️ Flat keys with dots can be un-nested

### Converting from PO to JSON
- ✅ Preserves keys and values
- ⚠️ Comments and context may be lost
- ⚠️ Plural forms become separate keys

### Converting from JSON to PO
- ✅ Preserves keys and values
- ❌ No context information
- ❌ No plural forms (unless encoded in keys)

### Converting to/from XLIFF
- ✅ Most comprehensive format
- ✅ Preserves source and target
- ✅ Preserves notes and comments
- ⚠️ Verbose output

### Converting SRT/VTT
- ✅ Can convert between each other
- ⚠️ Timing information preserved
- ⚠️ Cue IDs used as keys
- ❌ Not ideal for general localization

### Converting to/from CSV
- ✅ Easy bulk operations
- ⚠️ Multi-language CSV splits into separate files
- ❌ No nesting support
- ❌ No comments

---

## Feature Matrix

| Feature | JSON | YAML | RESX | PO | XLIFF | SRT | VTT | CSV | i18next | Fluent | VB |
|---------|------|------|------|----|----|-----|-----|-----|---------|--------|-----|
| **Read Support** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Write Support** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Nested Keys** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Comments** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ❌ | ✅ | ✅ |
| **Context** | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |
| **Plurals** | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ |
| **Metadata** | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ |
| **Multi-language** | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| **Tool Support** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **File Size** | Small | Small | Large | Medium | Large | Small | Small | Small | Small | Small | Large |
| **Readability** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| **Web Native** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ | ❌ |

---

## Recommended Format by Project Type

| Project Type | Primary Format | Alternative | Reason |
|--------------|---------------|-------------|---------|
| **React App** | i18next JSON | JSON | Native i18next support |
| **Vue.js App** | JSON | YAML | Simple and universal |
| **Angular App** | JSON | XLIFF | Native Angular i18n |
| **Node.js API** | JSON | YAML | Universal support |
| **.NET App** | RESX | JSON | Native .NET support |
| **Python App** | PO | JSON | Gettext integration |
| **PHP App** | PO | JSON | Gettext support |
| **Mobile App** | JSON | YAML | Cross-platform |
| **Video Platform** | SRT/VTT | - | Subtitle-specific |
| **Translation Team** | XLIFF | PO | Professional tools |
| **Open Source** | PO | JSON | Translation tools |
| **Documentation** | YAML | JSON | Readability |

---

## Additional Resources

- [API Reference](./API-REFERENCE.md)
- [Main README](../README.md)
- [Examples](../examples/)
- [Format-specific documentation](../docs/)