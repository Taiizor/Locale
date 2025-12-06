# Development Guide

Complete guide for setting up and developing Locale.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Building](#building)
- [Testing](#testing)
- [Debugging](#debugging)
- [Code Quality](#code-quality)
- [Adding New Features](#adding-new-features)
- [Release Process](#release-process)

---

## Prerequisites

### Required

- **.NET SDK 10.0** or later
  - Download from: https://dotnet.microsoft.com/download
  - Verify: `dotnet --version` should show 10.0.x or later

### Recommended

- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Git**: Version 2.30 or later
- **Node.js**: v18+ (for npm package testing)
- **Docker**: Optional, for containerized testing

### Optional Tools

- **BenchmarkDotNet**: For performance testing
- **dotnet-coverage**: For code coverage reports
- **dotnet-format**: For code formatting (included in SDK)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Taiizor/Locale.git
cd Locale
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Run the CLI

```bash
# Run directly without installing
dotnet run --project src/Locale.CLI -- scan ./samples --base en

# Or build and use the executable
dotnet build src/Locale.CLI
./src/Locale.CLI/bin/Debug/net10.0/Locale.CLI scan ./samples --base en
```

---

## Project Structure

```
Locale/
├── src/
│   ├── Locale/                    # Core library
│   │   ├── Formats/              # Format handlers
│   │   │   ├── ILocalizationFormat.cs
│   │   │   ├── LocalizationFormatBase.cs
│   │   │   ├── FormatRegistry.cs
│   │   │   ├── JsonLocalizationFormat.cs
│   │   │   ├── YamlLocalizationFormat.cs
│   │   │   └── ... (other formats)
│   │   ├── Models/               # Data models
│   │   │   ├── LocalizationFile.cs
│   │   │   ├── LocalizationEntry.cs
│   │   │   ├── ScanReport.cs
│   │   │   ├── DiffReport.cs
│   │   │   └── CheckReport.cs
│   │   ├── Services/             # Business logic
│   │   │   ├── ScanService.cs
│   │   │   ├── DiffService.cs
│   │   │   ├── CheckService.cs
│   │   │   ├── ConvertService.cs
│   │   │   ├── GenerateService.cs
│   │   │   ├── TranslateService.cs
│   │   │   └── WatchService.cs
│   │   └── Resources/            # Embedded resources
│   └── Locale.CLI/               # CLI tool
│       ├── Commands/             # CLI commands
│       │   ├── ScanCommand.cs
│       │   ├── DiffCommand.cs
│       │   ├── CheckCommand.cs
│       │   ├── ConvertCommand.cs
│       │   ├── GenerateCommand.cs
│       │   ├── TranslateCommand.cs
│       │   └── WatchCommand.cs
│       └── Program.cs            # CLI entry point
├── tests/
│   ├── Locale.Tests/             # Core library tests
│   │   ├── Formats/              # Format handler tests
│   │   ├── Services/             # Service tests
│   │   └── Models/               # Model tests
│   └── Locale.CLI.Tests/         # CLI tests
├── docs/                         # Documentation
│   ├── README.md
│   ├── API-REFERENCE.md
│   ├── FAQ.md
│   ├── TROUBLESHOOTING.md
│   ├── PERFORMANCE.md
│   ├── FORMAT-COMPARISON.md
│   ├── DEVELOPMENT.md
│   └── adr/                      # Architecture Decision Records
├── examples/                     # Usage examples
│   ├── basic-usage/
│   ├── format-conversion/
│   ├── ci-integration/
│   └── custom-format/
├── samples/                      # Sample localization files
│   └── locales/
├── npm/                          # npm package wrapper
│   ├── bin/
│   ├── lib/
│   └── package.json
├── .editorconfig                 # Editor configuration
├── .gitignore
├── Locale.slnx                   # Solution file
└── README.md
```

---

## Building

### Build All Projects

```bash
dotnet build
```

### Build Specific Project

```bash
dotnet build src/Locale
dotnet build src/Locale.CLI
```

### Build in Release Mode

```bash
dotnet build --configuration Release
```

### Clean Build

```bash
dotnet clean
dotnet build
```

### Build with Specific Target Framework

```bash
dotnet build --framework net10.0
dotnet build --framework net9.0
dotnet build --framework net8.0
```

---

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/Locale.Tests
dotnet test tests/Locale.CLI.Tests
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~JsonLocalizationFormatTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~JsonLocalizationFormatTests.Parse_ValidJson_ReturnsCorrectEntries"
```

### Run with Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports are in:
```
TestResults/{guid}/coverage.cobertura.xml
```

### View Coverage Report

Install ReportGenerator:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Generate HTML report:
```bash
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Open report
open coveragereport/index.html  # macOS
xdg-open coveragereport/index.html  # Linux
start coveragereport/index.html  # Windows
```

### Run Tests with Verbose Output

```bash
dotnet test --verbosity detailed
```

### Run Tests in Parallel

```bash
dotnet test --parallel
```

---

## Debugging

### Visual Studio

1. Open `Locale.slnx`
2. Set `Locale.CLI` as startup project
3. Set command-line arguments:
   - Right-click project → Properties → Debug
   - Application arguments: `scan ./samples --base en`
4. Press F5 to debug

### VS Code

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Debug CLI",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Locale.CLI/bin/Debug/net10.0/Locale.CLI.dll",
      "args": ["scan", "./samples", "--base", "en"],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "console": "internalConsole"
    },
    {
      "name": "Debug Tests",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "dotnet",
      "args": ["test", "tests/Locale.Tests"],
      "cwd": "${workspaceFolder}",
      "console": "internalConsole"
    }
  ]
}
```

### Rider

1. Open solution
2. Edit Configuration → Add → .NET Project
3. Set project: `Locale.CLI`
4. Program arguments: `scan ./samples --base en`
5. Click Debug

### Command Line Debugging

```bash
# Attach debugger
dotnet run --project src/Locale.CLI --no-build -- scan ./samples --base en

# Wait for debugger
DOTNET_STARTUP_HOOKS=Microsoft.Diagnostics.NETCore.Client \
  dotnet run --project src/Locale.CLI -- scan ./samples --base en
```

---

## Code Quality

### Code Formatting

```bash
# Check formatting
dotnet format --verify-no-changes

# Apply formatting
dotnet format
```

### Code Analysis

```bash
# Run analyzers
dotnet build /p:TreatWarningsAsErrors=true

# Run with specific analyzer level
dotnet build /p:AnalysisLevel=latest
```

### EditorConfig

Project uses `.editorconfig` for consistent formatting:
- Indent: 4 spaces
- Line endings: CRLF
- Nullable reference types: enabled
- Warnings as errors: enabled

### Code Style Guidelines

1. **Naming Conventions**
   - PascalCase for public members
   - camelCase for private fields with `_` prefix
   - PascalCase for types and namespaces

2. **Documentation**
   - XML comments required for public APIs
   - `<summary>`, `<param>`, `<returns>` tags
   - `<exception>` for documented exceptions

3. **Nullable Reference Types**
   - All projects have nullable enabled
   - Use `?` for nullable types
   - Use `!` only when absolutely necessary

4. **Error Handling**
   - Use appropriate exception types
   - Provide helpful error messages
   - See [ERROR-HANDLING.md](./ERROR-HANDLING.md)

5. **Async/Await**
   - Use `async`/`await` for I/O operations
   - Suffix async methods with `Async`
   - Accept `CancellationToken` parameter

---

## Adding New Features

### Adding a New Format

1. **Create format handler:**
   ```csharp
   // src/Locale/Formats/MyFormat.cs
   public sealed class MyFormatLocalizationFormat : LocalizationFormatBase
   {
       public override string FormatId => "myformat";
       public override IReadOnlyList<string> SupportedExtensions => [".myext"];
       
       public override LocalizationFile Parse(Stream stream, string? filePath = null)
       {
           // Implement parsing
       }
       
       public override void Write(LocalizationFile file, Stream stream)
       {
           // Implement writing
       }
   }
   ```

2. **Register in FormatRegistry:**
   ```csharp
   // src/Locale/Formats/FormatRegistry.cs
   private static FormatRegistry CreateDefault()
   {
       var registry = new FormatRegistry();
       registry.Register(new MyFormatLocalizationFormat());
       // ... other formats
       return registry;
   }
   ```

3. **Add tests:**
   ```csharp
   // tests/Locale.Tests/Formats/MyFormatTests.cs
   public class MyFormatLocalizationFormatTests
   {
       [Fact]
       public void FormatId_ReturnsMyFormat() { }
       
       [Fact]
       public void CanHandle_MyExtFile_ReturnsTrue() { }
       
       [Fact]
       public void Parse_ValidFile_ReturnsCorrectEntries() { }
       
       [Fact]
       public void Write_CreatesValidStructure() { }
       
       [Fact]
       public void Roundtrip_PreservesData() { }
   }
   ```

4. **Add sample file:**
   ```bash
   # samples/locales/en.myext
   ```

5. **Update documentation:**
   - Add to [FORMAT-COMPARISON.md](./FORMAT-COMPARISON.md)
   - Add to [README.md](../README.md)

### Adding a New Service

1. **Create service class:**
   ```csharp
   // src/Locale/Services/MyService.cs
   public sealed class MyService
   {
       private readonly FormatRegistry _registry;
       
       public MyService() : this(FormatRegistry.Default) { }
       public MyService(FormatRegistry registry) => _registry = registry;
       
       public MyReport DoOperation(string path, MyOptions options)
       {
           // Implementation
       }
   }
   ```

2. **Create options and report:**
   ```csharp
   public sealed class MyOptions
   {
       public required string RequiredOption { get; set; }
       public bool OptionalOption { get; set; }
   }
   
   public sealed class MyReport
   {
       public List<string> Results { get; init; } = [];
   }
   ```

3. **Add CLI command:**
   ```csharp
   // src/Locale.CLI/Commands/MyCommand.cs
   public sealed class MyCommand : Command<MyCommand.Settings>
   {
       public sealed class Settings : CommandSettings { }
       
       public override int Execute(CommandContext context, Settings settings)
       {
           // Implementation
       }
   }
   ```

4. **Register command:**
   ```csharp
   // src/Locale.CLI/Program.cs
   config.AddCommand<MyCommand>("my")
       .WithDescription("Does something useful");
   ```

5. **Add tests:**
   ```csharp
   // tests/Locale.Tests/Services/MyServiceTests.cs
   ```

### Adding a New Translation Provider

1. **Add to enum:**
   ```csharp
   // src/Locale/Services/TranslateService.cs
   public enum TranslationProvider
   {
       // ... existing
       MyProvider
   }
   ```

2. **Implement translation:**
   ```csharp
   private async Task<string> TranslateWithMyProviderAsync(...)
   {
       // Implementation
   }
   ```

3. **Update switch statement:**
   ```csharp
   var translated = options.Provider switch
   {
       // ... existing
       TranslationProvider.MyProvider => await TranslateWithMyProviderAsync(...),
       _ => throw new NotSupportedException()
   };
   ```

4. **Add documentation:**
   - Update [README.md](../README.md) provider table
   - Update [FAQ.md](./FAQ.md)

---

## Release Process

### 1. Update Version

Update version in all `.csproj` files:
```xml
<Version>0.0.12</Version>
<AssemblyVersion>0.0.12</AssemblyVersion>
```

And in `npm/package.json`:
```json
{
  "version": "0.0.12"
}
```

### 2. Update CHANGELOG

Add release notes to [CHANGELOG.md](../CHANGELOG.md):
```markdown
## [0.0.12] - 2024-12-07

### Added
- New feature X
- New format Y support

### Fixed
- Bug Z in scanning

### Changed
- Improved performance of conversion
```

### 3. Create Git Tag

```bash
git tag -a v0.0.12 -m "Release v0.0.12"
git push origin v0.0.12
```

### 4. GitHub Release

GitHub Actions automatically:
1. Builds release artifacts
2. Publishes to NuGet
3. Creates GitHub release
4. Publishes to npm

### 5. Verify Release

```bash
# Check NuGet
dotnet tool install -g Locale.CLI --version 0.0.12

# Check npm
npm install -g @taiizor/locale-cli@0.0.12
```

---

## Troubleshooting Development Issues

### Build Fails

```bash
# Clear build artifacts
dotnet clean
rm -rf */bin */obj

# Restore and rebuild
dotnet restore
dotnet build
```

### Tests Fail

```bash
# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Enable verbose output
dotnet test --verbosity detailed
```

### IDE Issues

**Visual Studio:**
- Tools → Options → Environment → Preview Features
- Enable "Use previews of the .NET SDK"

**VS Code:**
- Install C# extension
- Reload window after installing SDK

**Rider:**
- File → Invalidate Caches / Restart

### Package Restore Issues

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed
```

---

## Additional Resources

- [Contributing Guide](../.github/CONTRIBUTING.md)
- [Code of Conduct](../.github/CODE_OF_CONDUCT.md)
- [Architecture Decision Records](./adr/)
- [Error Handling Guidelines](./ERROR-HANDLING.md)
- [Performance Guide](./PERFORMANCE.md)

---

## Getting Help

- **Issues:** https://github.com/Taiizor/Locale/issues
- **Discussions:** https://github.com/Taiizor/Locale/discussions
- **Discord:** https://discord.gg/nxG977byXb
- **Email:** taiizor@vegalya.com