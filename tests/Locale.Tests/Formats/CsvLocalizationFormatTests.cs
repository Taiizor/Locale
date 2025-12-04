using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class CsvLocalizationFormatTests
{
    private readonly CsvLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsCsv()
    {
        Assert.Equal("csv", _format.FormatId);
    }

    [Fact]
    public void CanHandle_CsvFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.csv"));
        Assert.True(_format.CanHandle("path/to/translations.csv"));
    }

    [Fact]
    public void Parse_TwoColumnCsv_ReturnsCorrectEntries()
    {
        string csv = """
        key,value
        hello,Hello World
        welcome,Welcome!
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(csv));
        LocalizationFile file = _format.Parse(stream, "en.csv");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("hello"));
        Assert.Equal("Welcome!", file.GetValue("welcome"));
    }

    [Fact]
    public void Parse_QuotedValues_HandlesCorrectly()
    {
        string csv = "key,value\nhello,\"Hello, World\"\nquote,\"Say \"\"Hello\"\"\"";

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(csv));
        LocalizationFile file = _format.Parse(stream);

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello, World", file.GetValue("hello"));
        Assert.Equal("Say \"Hello\"", file.GetValue("quote"));
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.csv",
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