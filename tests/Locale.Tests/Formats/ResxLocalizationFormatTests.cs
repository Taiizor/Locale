using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class ResxLocalizationFormatTests
{
    private readonly ResxLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsResx()
    {
        Assert.Equal("resx", _format.FormatId);
    }

    [Fact]
    public void CanHandle_ResxFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.resx"));
        Assert.True(_format.CanHandle("path/to/Resources.en.resx"));
    }

    [Fact]
    public void CanHandle_NonResxFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.json"));
        Assert.False(_format.CanHandle("test.xml"));
    }

    [Fact]
    public void Parse_ValidResx_ReturnsCorrectEntries()
    {
        string resx = """
        <?xml version="1.0" encoding="utf-8"?>
        <root>
          <data name="Hello" xml:space="preserve">
            <value>Hello World</value>
            <comment>Greeting message</comment>
          </data>
          <data name="Welcome_Message" xml:space="preserve">
            <value>Welcome!</value>
          </data>
        </root>
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(resx));
        LocalizationFile file = _format.Parse(stream, "en.resx");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("Hello"));
        Assert.Equal("Welcome!", file.GetValue("Welcome.Message")); // Underscore converted to dot
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.resx",
            Entries =
            [
                new LocalizationEntry { Key = "Hello", Value = "Hello World" },
                new LocalizationEntry { Key = "Nested.Key", Value = "Nested Value" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("Hello World", parsed.GetValue("Hello"));
        Assert.Equal("Nested Value", parsed.GetValue("Nested.Key"));
    }
}