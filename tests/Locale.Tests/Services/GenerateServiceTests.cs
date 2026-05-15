using Locale.Formats;
using Locale.Models;
using Locale.Services;

namespace Locale.Tests.Services;

public class GenerateServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly GenerateService _service;

    public GenerateServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"locale_generate_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _service = new GenerateService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithRegistry_ShouldWork()
    {
        // Arrange & Act
        GenerateService service = new(FormatRegistry.Default);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_Default_ShouldWork()
    {
        // Arrange & Act
        GenerateService service = new();

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Generate_WithSingleFile_ShouldCreateTargetFile()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "messages.en.json");
        string outputDir = Path.Combine(_testDir, "output");
        Directory.CreateDirectory(outputDir);

        string jsonContent = """
            {
                "Welcome": "Welcome to our application",
                "Goodbye": "Thank you for using our app"
            }
            """;
        File.WriteAllText(sourceFile, jsonContent);

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            PlaceholderPattern = "@@MISSING@@ {0}"
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        GenerateResult result = results[0];
        Assert.True(result.Success, result.ErrorMessage);
        Assert.True(result.Created);
        Assert.Equal(2, result.KeysAdded);
        Assert.Equal(0, result.KeysSkipped);
        Assert.True(File.Exists(result.FilePath));
    }

    [Fact]
    public void Generate_WithDirectory_ShouldGenerateForAllFiles()
    {
        // Arrange
        string sourceDir = _testDir;
        string outputDir = Path.Combine(_testDir, "output");
        Directory.CreateDirectory(outputDir);

        File.WriteAllText(Path.Combine(sourceDir, "app.en.json"), "{\"Title\": \"App\"}");
        File.WriteAllText(Path.Combine(sourceDir, "errors.en.json"), "{\"NotFound\": \"Not Found\"}");

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "de",
            Recursive = false
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceDir, outputDir, options);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success, r.ErrorMessage));
    }

    [Fact]
    public void Generate_WithExistingTargetFile_ShouldUpdateFile()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "test.en.json");
        string outputDir = _testDir;

        // Create base file with 3 keys
        File.WriteAllText(sourceFile, """
            {
                "Key1": "Value1",
                "Key2": "Value2",
                "Key3": "Value3"
            }
            """);

        // Create existing target file with 1 key
        string targetFile = Path.Combine(outputDir, "test.tr.json");
        File.WriteAllText(targetFile, """
            {
                "Key1": "Değer1"
            }
            """);

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            OverwriteExisting = false
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        GenerateResult result = results[0];
        Assert.True(result.Success, result.ErrorMessage);
        Assert.False(result.Created);
        Assert.Equal(2, result.KeysAdded);
        Assert.Equal(1, result.KeysSkipped);
    }

    [Fact]
    public void Generate_WithOverwriteExisting_ShouldReplaceAllKeys()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "test.en.json");
        string outputDir = _testDir;

        File.WriteAllText(sourceFile, """
            {
                "Key1": "Value1",
                "Key2": "Value2"
            }
            """);

        string targetFile = Path.Combine(outputDir, "test.tr.json");
        File.WriteAllText(targetFile, """
            {
                "Key1": "ExistingValue"
            }
            """);

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            OverwriteExisting = true
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        GenerateResult result = results[0];
        Assert.True(result.Success, result.ErrorMessage);
        Assert.Equal(2, result.KeysAdded);
        Assert.Equal(0, result.KeysSkipped);
    }

    [Fact]
    public void Generate_WithUseEmptyValue_ShouldCreateEmptyStrings()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "test.en.json");
        string outputDir = _testDir;

        File.WriteAllText(sourceFile, """
            {
                "Key1": "Value1"
            }
            """);

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            UseEmptyValue = true
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        GenerateResult result = results[0];
        Assert.True(result.Success, result.ErrorMessage);

        // Verify empty values
        JsonLocalizationFormat format = new();
        LocalizationFile targetFile = format.Parse(result.FilePath);
        Assert.All(targetFile.Entries, e => Assert.Equal("", e.Value));
    }

    [Fact]
    public void Generate_WithCustomPlaceholder_ShouldUsePlaceholder()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "test.en.json");
        string outputDir = _testDir;

        File.WriteAllText(sourceFile, """
            {
                "Key1": "Value1"
            }
            """);

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            PlaceholderPattern = "[TODO: {0}]"
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        GenerateResult result = results[0];
        Assert.True(result.Success, result.ErrorMessage);

        // Verify placeholder
        JsonLocalizationFormat format = new();
        LocalizationFile targetFile = format.Parse(result.FilePath);
        Assert.Contains("[TODO: Value1]", targetFile.Entries[0].Value);
    }

    [Fact]
    public void Generate_WithRecursive_ShouldProcessSubdirectories()
    {
        // Arrange
        string sourceDir = _testDir;
        string subDir = Path.Combine(sourceDir, "sub");
        Directory.CreateDirectory(subDir);
        string outputDir = Path.Combine(_testDir, "output");

        File.WriteAllText(Path.Combine(sourceDir, "root.en.json"), "{\"Root\": \"Value\"}");
        File.WriteAllText(Path.Combine(subDir, "sub.en.json"), "{\"Sub\": \"Value\"}");

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "fr",
            Recursive = true
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceDir, outputDir, options);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success, r.ErrorMessage));
    }

    [Fact]
    public void Generate_WithNonRecursive_ShouldNotProcessSubdirectories()
    {
        // Arrange
        string sourceDir = _testDir;
        string subDir = Path.Combine(sourceDir, "sub");
        Directory.CreateDirectory(subDir);
        string outputDir = Path.Combine(_testDir, "output");

        File.WriteAllText(Path.Combine(sourceDir, "root.en.json"), "{\"Root\": \"Value\"}");
        File.WriteAllText(Path.Combine(subDir, "sub.en.json"), "{\"Sub\": \"Value\"}");

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "fr",
            Recursive = false
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceDir, outputDir, options);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void Generate_WithNonMatchingBaseCulture_ShouldSkipFile()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "test.de.json");
        string outputDir = _testDir;

        File.WriteAllText(sourceFile, "{\"Key\": \"Value\"}");

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr"
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Generate_WithNonexistentPath_ShouldReturnError()
    {
        // Arrange
        string nonexistentPath = Path.Combine(_testDir, "nonexistent");
        string outputDir = _testDir;

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr"
        };

        // Act
        List<GenerateResult> results = _service.Generate(nonexistentPath, outputDir, options);

        // Assert
        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.Contains("does not exist", results[0].ErrorMessage);
    }

    [Fact]
    public void Generate_WithInvalidJson_ShouldReturnError()
    {
        // Arrange
        string sourceFile = Path.Combine(_testDir, "invalid.en.json");
        string outputDir = _testDir;

        File.WriteAllText(sourceFile, "{ invalid json }");

        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr"
        };

        // Act
        List<GenerateResult> results = _service.Generate(sourceFile, outputDir, options);

        // Assert
        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.Contains("Failed to parse", results[0].ErrorMessage);
    }

    [Fact]
    public void GenerateOptions_Properties_ShouldWorkAsExpected()
    {
        // Arrange & Act
        GenerateOptions options = new()
        {
            BaseCulture = "en",
            TargetCulture = "tr",
            PlaceholderPattern = "MISSING: {0}",
            UseEmptyValue = true,
            OverwriteExisting = true,
            Recursive = false
        };

        // Assert
        Assert.Equal("en", options.BaseCulture);
        Assert.Equal("tr", options.TargetCulture);
        Assert.Equal("MISSING: {0}", options.PlaceholderPattern);
        Assert.True(options.UseEmptyValue);
        Assert.True(options.OverwriteExisting);
        Assert.False(options.Recursive);
    }

    [Fact]
    public void GenerateResult_Properties_ShouldWorkAsExpected()
    {
        // Arrange & Act
        GenerateResult result = new()
        {
            FilePath = "test.json",
            Created = true,
            KeysAdded = 5,
            KeysSkipped = 2,
            ErrorMessage = null
        };

        // Assert
        Assert.Equal("test.json", result.FilePath);
        Assert.True(result.Created);
        Assert.Equal(5, result.KeysAdded);
        Assert.Equal(2, result.KeysSkipped);
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void GenerateResult_Success_WithErrorMessage_ShouldReturnFalse()
    {
        // Arrange & Act
        GenerateResult result = new()
        {
            FilePath = "test.json",
            ErrorMessage = "Some error"
        };

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void Generate_NeutralResxAsBase_GeneratesTargetFile()
    {
        // Issue #24: A neutral Resources.resx (no culture suffix) holding the
        // base-language strings should be picked up when --base/--from is set.
        string baseFile = Path.Combine(_testDir, "Resources.resx");

        File.WriteAllText(baseFile, """
            <?xml version="1.0" encoding="utf-8"?>
            <root>
              <data name="Welcome"><value>Willkommen</value></data>
              <data name="Goodbye"><value>Auf Wiedersehen</value></data>
            </root>
            """);

        GenerateOptions options = new()
        {
            BaseCulture = "de",
            TargetCulture = "en",
            UseEmptyValue = true
        };

        List<GenerateResult> results = _service.Generate(_testDir, _testDir, options);

        GenerateResult result = Assert.Single(results);
        Assert.True(result.Success);
        Assert.True(result.Created);
        Assert.Equal(2, result.KeysAdded);

        // The output filename follows the suffix-less pattern: Resources.en.resx
        Assert.Equal("Resources.en.resx", Path.GetFileName(result.FilePath));
        Assert.True(File.Exists(result.FilePath));
    }
}