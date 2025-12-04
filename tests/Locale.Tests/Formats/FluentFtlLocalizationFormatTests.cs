using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class FluentFtlLocalizationFormatTests
{
    private readonly FluentFtlLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsFtl()
    {
        Assert.Equal("ftl", _format.FormatId);
    }

    [Fact]
    public void CanHandle_FtlFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.ftl"));
        Assert.True(_format.CanHandle("path/to/messages.ftl"));
    }

    [Fact]
    public void CanHandle_NonFtlFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.json"));
        Assert.False(_format.CanHandle("test.po"));
    }

    [Fact]
    public void Parse_SimpleMessages_ReturnsCorrectEntries()
    {
        string ftl = """
        hello = Hello World
        welcome = Welcome to our app
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(ftl));
        LocalizationFile file = _format.Parse(stream, "en.ftl");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("hello"));
        Assert.Equal("Welcome to our app", file.GetValue("welcome"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Parse_WithComments_PreservesComments()
    {
        string ftl = """
        # This is a greeting
        hello = Hello World
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(ftl));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Equal("Hello World", file.GetValue("hello"));
        Assert.Equal("This is a greeting", file.Entries[0].Comment);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.ftl",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello World" },
                new LocalizationEntry { Key = "goodbye", Value = "Goodbye!" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("Hello World", parsed.GetValue("hello"));
        Assert.Equal("Goodbye!", parsed.GetValue("goodbye"));
    }
}