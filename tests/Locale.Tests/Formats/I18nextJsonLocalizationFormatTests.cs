using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class I18nextJsonLocalizationFormatTests
{
    private readonly I18nextJsonLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsI18next()
    {
        Assert.Equal("i18next", _format.FormatId);
    }

    [Fact]
    public void CanHandle_I18nextFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.i18n.json"));
        Assert.True(_format.CanHandle("en.i18n.json"));
        Assert.True(_format.CanHandle("translation.json"));
    }

    [Fact]
    public void Parse_NestedJson_FlattensKeys()
    {
        string json = """
        {
            "common": {
                "hello": "Hello",
                "welcome": "Welcome"
            },
            "home": {
                "title": "Home"
            }
        }
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(json));
        LocalizationFile file = _format.Parse(stream, "en.i18n.json");

        Assert.Equal(3, file.Entries.Count);
        Assert.Equal("Hello", file.GetValue("common.hello"));
        Assert.Equal("Welcome", file.GetValue("common.welcome"));
        Assert.Equal("Home", file.GetValue("home.title"));
        Assert.Equal("en", file.Culture);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.i18n.json",
            Entries =
            [
                new LocalizationEntry { Key = "common.hello", Value = "Hello World" },
                new LocalizationEntry { Key = "common.goodbye", Value = "Goodbye" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Equal(2, parsed.Entries.Count);
        Assert.Equal("Hello World", parsed.GetValue("common.hello"));
        Assert.Equal("Goodbye", parsed.GetValue("common.goodbye"));
    }
}