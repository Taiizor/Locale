using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class JsonLocalizationFormatTests
{
    private readonly JsonLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsJson()
    {
        Assert.Equal("json", _format.FormatId);
    }

    [Fact]
    public void CanHandle_JsonFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.json"));
        Assert.True(_format.CanHandle("path/to/file.json"));
    }

    [Fact]
    public void CanHandle_NonJsonFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.yaml"));
        Assert.False(_format.CanHandle("test.resx"));
    }

    [Fact]
    public void Parse_FlatJson_ReturnsCorrectEntries()
    {
        string json = """
        {
            "hello": "Hello",
            "world": "World"
        }
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json));
        LocalizationFile file = _format.Parse(stream, "en.json");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello", file.GetValue("hello"));
        Assert.Equal("World", file.GetValue("world"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Parse_NestedJson_FlattensKeys()
    {
        string json = """
        {
            "home": {
                "title": "Home",
                "subtitle": "Welcome"
            }
        }
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json));
        LocalizationFile file = _format.Parse(stream);

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Home", file.GetValue("home.title"));
        Assert.Equal("Welcome", file.GetValue("home.subtitle"));
    }

    [Fact]
    public void Write_CreatesNestedStructure()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new LocalizationEntry { Key = "home.title", Value = "Home" },
                new LocalizationEntry { Key = "home.subtitle", Value = "Welcome" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();

        Assert.Contains("\"home\"", json);
        Assert.Contains("\"title\"", json);
        Assert.Contains("\"Home\"", json);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.json",
            Entries =
            [
                new LocalizationEntry { Key = "simple", Value = "Simple Value" },
                new LocalizationEntry { Key = "nested.key", Value = "Nested Value" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("Simple Value", parsed.GetValue("simple"));
        Assert.Equal("Nested Value", parsed.GetValue("nested.key"));
    }
}