using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class SrtLocalizationFormatTests
{
    private readonly SrtLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsSrt()
    {
        Assert.Equal("srt", _format.FormatId);
    }

    [Fact]
    public void CanHandle_SrtFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.srt"));
        Assert.True(_format.CanHandle("path/to/subs.en.srt"));
    }

    [Fact]
    public void Parse_ValidSrt_ReturnsCorrectEntries()
    {
        string srt = """
        1
        00:00:01,000 --> 00:00:04,000
        Hello World

        2
        00:00:05,000 --> 00:00:08,000
        Welcome to the show!

        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(srt));
        LocalizationFile file = _format.Parse(stream, "en.srt");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("1"));
        Assert.Equal("Welcome to the show!", file.GetValue("2"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.srt",
            Entries =
            [
                new LocalizationEntry { Key = "1", Value = "First subtitle", Comment = "00:00:01,000 --> 00:00:04,000" },
                new LocalizationEntry { Key = "2", Value = "Second subtitle", Comment = "00:00:05,000 --> 00:00:08,000" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("First subtitle", parsed.GetValue("1"));
        Assert.Equal("Second subtitle", parsed.GetValue("2"));
    }
}