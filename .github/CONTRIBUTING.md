# Contributing to Locale

Thank you for your interest in contributing to Locale! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Setting Up the Development Environment

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/Taiizor/Locale.git
   cd Locale
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the solution:
   ```bash
   dotnet build
   ```
5. Run tests:
   ```bash
   dotnet test
   ```

## 📝 Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation to all public APIs
- Keep methods focused and small
- Use nullable reference types

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `LocalizationEntry` |
| Interfaces | IPascalCase | `ILocalizationFormat` |
| Methods | PascalCase | `ParseFile()` |
| Properties | PascalCase | `FormatId` |
| Private fields | _camelCase | `_entries` |
| Parameters | camelCase | `filePath` |
| Constants | PascalCase | `DefaultCulture` |

## 🔧 Types of Contributions

### Bug Fixes

1. Check if the bug is already reported
2. Create an issue if it doesn't exist
3. Fork and create a branch: `fix/issue-description`
4. Write tests that reproduce the bug
5. Fix the bug
6. Ensure all tests pass
7. Submit a PR

### New Features

1. Open an issue to discuss the feature
2. Wait for approval from maintainers
3. Fork and create a branch: `feature/feature-name`
4. Implement the feature with tests
5. Update documentation
6. Submit a PR

### New File Formats

To add a new localization format:

1. Create a new class in `src/Locale/Formats/`:
   ```csharp
   public sealed class MyFormat : LocalizationFormatBase
   {
       public override string FormatId => "myformat";
       public override IReadOnlyList<string> SupportedExtensions => [".myext"];
       
       public override LocalizationFile Parse(Stream stream, string? filePath = null)
       {
           // Implementation
       }
       
       public override void Write(LocalizationFile file, Stream stream)
       {
           // Implementation
       }
   }
   ```

2. Register in `FormatRegistry.CreateDefault()`

3. Add tests in `tests/Locale.Tests/Formats/`

4. Update documentation

### New Translation Providers

To add a new translation provider:

1. Add the provider enum value in `TranslationProvider`

2. Implement the translation logic in `TranslateService`

3. Add CLI support in `TranslateCommand`

4. Add tests

5. Update documentation

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/Locale.Tests/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Writing Tests

- Use xUnit for unit tests
- Follow the Arrange-Act-Assert pattern
- Use meaningful test method names
- Test edge cases and error conditions

```csharp
[Fact]
public void Parse_ValidJson_ReturnsLocalizationFile()
{
    // Arrange
    var format = new JsonLocalizationFormat();
    var json = """{"key": "value"}""";
    
    // Act
    var file = format.Parse(json);
    
    // Assert
    Assert.Single(file.Entries);
    Assert.Equal("key", file.Entries[0].Key);
}
```

## 📦 Pull Request Process

1. Update the README.md with details of changes if applicable
2. Update XML documentation for any new public APIs
3. Ensure all tests pass
4. Update the version number if applicable
5. The PR will be merged once approved by maintainers

### PR Title Format

Use conventional commits format:
- `fix: description` for bug fixes
- `feat: description` for new features
- `docs: description` for documentation
- `refactor: description` for refactoring
- `test: description` for tests
- `chore: description` for maintenance

## 🏗️ Project Structure

```
Locale/
├── src/
│   ├── Locale/                 # Core library
│   │   ├── Models/             # Data models
│   │   ├── Formats/            # Format handlers
│   │   └── Services/           # Business logic
│   └── Locale.CLI/             # CLI tool
│       └── Commands/           # CLI commands
├── tests/
│   ├── Locale.Tests/           # Core library tests
│   └── Locale.CLI.Tests/       # CLI tests
└── .github/                    # GitHub configuration
```

## 📜 License

By contributing, you agree that your contributions will be licensed under the MIT License.

## 💬 Questions?

- Open a [Discussion](https://github.com/Taiizor/Locale/discussions)
- Check existing [Issues](https://github.com/Taiizor/Locale/issues)

Thank you for contributing! 🎉
