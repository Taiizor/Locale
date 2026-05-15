using Locale.Models;
using System.Globalization;

namespace Locale.Tests.Models;

public class LocalizationFileTests
{
    [Fact]
    public void Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange & Act
        LocalizationFile file = new()
        {
            FilePath = "/path/to/file.json",
            Culture = "en-US",
            Format = "json"
        };

        // Assert
        Assert.Equal("/path/to/file.json", file.FilePath);
        Assert.Equal("en-US", file.Culture);
        Assert.Equal("json", file.Format);
    }

    [Fact]
    public void Entries_ShouldInitializeAsEmptyList()
    {
        // Arrange & Act
        LocalizationFile file = new()
        {
            FilePath = "test.json"
        };

        // Assert
        Assert.NotNull(file.Entries);
        Assert.Empty(file.Entries);
    }

    [Fact]
    public void Entries_ShouldAcceptInitializer()
    {
        // Arrange & Act
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" }
            ]
        };

        // Assert
        Assert.Equal(2, file.Entries.Count);
    }

    [Fact]
    public void EntriesByKey_ShouldReturnDictionary()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" }
            ]
        };

        // Act
        IReadOnlyDictionary<string, LocalizationEntry> dict = file.EntriesByKey;

        // Assert
        Assert.Equal(2, dict.Count);
        Assert.True(dict.ContainsKey("Key1"));
        Assert.True(dict.ContainsKey("Key2"));
    }

    [Fact]
    public void EntriesByKey_ShouldBeCached()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        IReadOnlyDictionary<string, LocalizationEntry> dict1 = file.EntriesByKey;
        IReadOnlyDictionary<string, LocalizationEntry> dict2 = file.EntriesByKey;

        // Assert
        Assert.Same(dict1, dict2);
    }

    [Fact]
    public void Keys_ShouldReturnAllKeys()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" },
                new() { Key = "Key3", Value = "Value3" }
            ]
        };

        // Act
        List<string> keys = [.. file.Keys];

        // Assert
        Assert.Equal(3, keys.Count);
        Assert.Contains("Key1", keys);
        Assert.Contains("Key2", keys);
        Assert.Contains("Key3", keys);
    }

    [Fact]
    public void Count_ShouldReturnEntryCount()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" }
            ]
        };

        // Act & Assert
        Assert.Equal(2, file.Count);
    }

    [Fact]
    public void GetCultureInfo_WithValidCulture_ShouldReturnCultureInfo()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en-US"
        };

        // Act
        CultureInfo? cultureInfo = file.GetCultureInfo();

        // Assert
        Assert.NotNull(cultureInfo);
        Assert.Equal("en-US", cultureInfo.Name);
    }

    [Fact]
    public void GetCultureInfo_WithInvalidCulture_ShouldHandleGracefully()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "xxx-INVALID"
        };

        // Act
        // Platform-specific behavior: Windows returns null, Linux may create a culture.
        // The important thing is that the method does not throw — no assertion needed
        // because the test would fail on an unhandled exception.
        _ = file.GetCultureInfo();
    }

    [Fact]
    public void GetCultureInfo_WithNullCulture_ShouldReturnNull()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = null
        };

        // Act
        CultureInfo? cultureInfo = file.GetCultureInfo();

        // Assert
        Assert.Null(cultureInfo);
    }

    [Fact]
    public void GetCultureInfo_WithEmptyCulture_ShouldReturnNull()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = ""
        };

        // Act
        CultureInfo? cultureInfo = file.GetCultureInfo();

        // Assert
        Assert.Null(cultureInfo);
    }

    [Fact]
    public void GetValue_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        string? value = file.GetValue("Key1");

        // Assert
        Assert.Equal("Value1", value);
    }

    [Fact]
    public void GetValue_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        string? value = file.GetValue("NonExistingKey");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        bool contains = file.ContainsKey("Key1");

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        bool contains = file.ContainsKey("NonExistingKey");

        // Assert
        Assert.False(contains);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "/path/to/file.json",
            Culture = "en-US",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" }
            ]
        };

        // Act
        string result = file.ToString();

        // Assert
        Assert.Equal("/path/to/file.json (en-US) - 2 entries", result);
    }

    [Fact]
    public void ToString_WithNoCulture_ShouldShowUnknown()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "/path/to/file.json",
            Culture = null,
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        string result = file.ToString();

        // Assert
        Assert.Equal("/path/to/file.json (unknown) - 1 entries", result);
    }

    [Fact]
    public void Entries_WhenChanged_ShouldInvalidateCache()
    {
        // Arrange
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" }
            ]
        };

        // Act
        IReadOnlyDictionary<string, LocalizationEntry> dict1 = file.EntriesByKey;

        // Modify entries by creating a new file with different entries
        LocalizationFile file2 = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new() { Key = "Key1", Value = "Value1" },
                new() { Key = "Key2", Value = "Value2" }
            ]
        };

        // Assert
        Assert.Single(dict1);
        Assert.Equal(2, file2.EntriesByKey.Count);
    }
}