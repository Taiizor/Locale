using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class PoLocalizationFormatTests
{
    private readonly PoLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsPo()
    {
        Assert.Equal("po", _format.FormatId);
    }

    [Fact]
    public void CanHandle_PoFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.po"));
        Assert.True(_format.CanHandle("path/to/messages.po"));
    }

    [Fact]
    public void CanHandle_NonPoFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.json"));
        Assert.False(_format.CanHandle("test.pot"));
    }

    [Fact]
    public void Parse_ValidPo_ReturnsCorrectEntries()
    {
        string po = """
        msgid ""
        msgstr ""
        "Content-Type: text/plain; charset=UTF-8\n"

        msgid "Hello"
        msgstr "Hello World"

        msgid "Welcome"
        msgstr "Welcome!"
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(po));
        LocalizationFile file = _format.Parse(stream, "en.po");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("Hello"));
        Assert.Equal("Welcome!", file.GetValue("Welcome"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.po",
            Entries =
            [
                new LocalizationEntry { Key = "Hello", Value = "Hello World" },
                new LocalizationEntry { Key = "Goodbye", Value = "Goodbye!" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("Hello World", parsed.GetValue("Hello"));
        Assert.Equal("Goodbye!", parsed.GetValue("Goodbye"));
    }
}