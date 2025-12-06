using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class VttLocalizationFormatTests
{
    private readonly VttLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsVtt()
    {
        Assert.Equal("vtt", _format.FormatId);
    }

    [Fact]
    public void CanHandle_VttFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.vtt"));
        Assert.True(_format.CanHandle("path/to/subtitles.vtt"));
    }

    [Fact]
    public void CanHandle_NonVttFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.srt"));
        Assert.False(_format.CanHandle("test.json"));
    }

    [Fact]
    public void Parse_SimpleVtt_ReturnsCorrectEntries()
    {
        string vtt = """
            WEBVTT

            00:00:01.000 --> 00:00:02.000
            Hello World

            00:00:03.000 --> 00:00:04.000
            Second subtitle
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vtt));
        LocalizationFile file = _format.Parse(stream, "en.vtt");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Hello World", file.GetValue("1"));
        Assert.Equal("Second subtitle", file.GetValue("2"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Parse_VttWithCueIdentifiers_UsesCueIdAsKey()
    {
        string vtt = """
            WEBVTT

            intro
            00:00:01.000 --> 00:00:02.000
            Welcome to the show

            outro
            00:00:03.000 --> 00:00:04.000
            Thanks for watching
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vtt));
        LocalizationFile file = _format.Parse(stream);

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Welcome to the show", file.GetValue("intro"));
        Assert.Equal("Thanks for watching", file.GetValue("outro"));
    }

    [Fact]
    public void Parse_VttWithMultilineText_PreservesLineBreaks()
    {
        string vtt = """
            WEBVTT

            00:00:01.000 --> 00:00:02.000
            Line 1
            Line 2
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vtt));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Contains("Line 1", file.Entries[0].Value);
        Assert.Contains("Line 2", file.Entries[0].Value);
    }

    [Fact]
    public void Parse_VttWithTiming_StoresTimingAsComment()
    {
        string vtt = """
            WEBVTT

            00:00:05.500 --> 00:00:10.200
            Test subtitle
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vtt));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Contains("00:00:05.500", file.Entries[0].Comment);
        Assert.Contains("00:00:10.200", file.Entries[0].Comment);
    }

    [Fact]
    public void Write_CreatesValidVttStructure()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.vtt",
            Entries =
            [
                new LocalizationEntry { Key = "1", Value = "Hello", Comment = "00:00:01.000 --> 00:00:02.000" },
                new LocalizationEntry { Key = "2", Value = "World", Comment = "00:00:03.000 --> 00:00:04.000" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string vtt = reader.ReadToEnd();

        Assert.StartsWith("WEBVTT", vtt);
        Assert.Contains("Hello", vtt);
        Assert.Contains("World", vtt);
        Assert.Contains("00:00:01.000 --> 00:00:02.000", vtt);
    }

    [Fact]
    public void Write_WithCustomCueId_IncludesCueId()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.vtt",
            Entries =
            [
                new LocalizationEntry { Key = "intro", Value = "Welcome", Comment = "00:00:01.000 --> 00:00:02.000" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string vtt = reader.ReadToEnd();

        Assert.Contains("intro", vtt);
        Assert.Contains("Welcome", vtt);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.vtt",
            Entries =
            [
                new LocalizationEntry { Key = "greeting", Value = "Hello World", Comment = "00:00:01.000 --> 00:00:02.000" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Single(parsed.Entries);
        Assert.Equal("greeting", parsed.Entries[0].Key);
        Assert.Equal("Hello World", parsed.Entries[0].Value);
    }
}