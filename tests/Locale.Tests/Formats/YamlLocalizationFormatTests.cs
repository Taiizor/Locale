using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class YamlLocalizationFormatTests
{
    private readonly YamlLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsYaml()
    {
        Assert.Equal("yaml", _format.FormatId);
    }

    [Fact]
    public void CanHandle_YamlFiles_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.yaml"));
        Assert.True(_format.CanHandle("test.yml"));
    }

    [Fact]
    public void CanHandle_NonYamlFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.json"));
        Assert.False(_format.CanHandle("test.resx"));
    }

    [Fact]
    public void Parse_FlatYaml_ReturnsCorrectEntries()
    {
        string yaml = """
        hello: Hello
        world: World
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(yaml));
        LocalizationFile file = _format.Parse(stream, "en.yaml");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello", file.GetValue("hello"));
        Assert.Equal("World", file.GetValue("world"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Parse_NestedYaml_FlattensKeys()
    {
        string yaml = """
        home:
          title: Home
          subtitle: Welcome
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(yaml));
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
            FilePath = "test.yaml",
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
        string yaml = reader.ReadToEnd();

        Assert.Contains("home:", yaml);
        Assert.Contains("title:", yaml);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.yaml",
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