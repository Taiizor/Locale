using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class XliffLocalizationFormatTests
{
    private readonly XliffLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsXliff()
    {
        Assert.Equal("xliff", _format.FormatId);
    }

    [Fact]
    public void CanHandle_XlfFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.xlf"));
        Assert.True(_format.CanHandle("test.xliff"));
        Assert.True(_format.CanHandle("path/to/file.xlf"));
    }

    [Fact]
    public void CanHandle_NonXliffFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.json"));
        Assert.False(_format.CanHandle("test.xml"));
    }

    [Fact]
    public void Parse_Xliff12_ReturnsCorrectEntries()
    {
        string xliff = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
              <file source-language="en" target-language="tr" datatype="plaintext">
                <body>
                  <trans-unit id="greeting">
                    <source>Hello</source>
                    <target>Merhaba</target>
                  </trans-unit>
                  <trans-unit id="farewell">
                    <source>Goodbye</source>
                    <target>Hoşçakal</target>
                  </trans-unit>
                </body>
              </file>
            </xliff>
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xliff));
        LocalizationFile file = _format.Parse(stream, "tr.xlf");

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("Merhaba", file.GetValue("greeting"));
        Assert.Equal("Hoşçakal", file.GetValue("farewell"));
        Assert.Equal("tr", file.Culture);
    }

    [Fact]
    public void Parse_Xliff12_PreservesSourceText()
    {
        string xliff = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
              <file source-language="en" target-language="de" datatype="plaintext">
                <body>
                  <trans-unit id="title">
                    <source>Welcome</source>
                    <target>Willkommen</target>
                  </trans-unit>
                </body>
              </file>
            </xliff>
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xliff));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Equal("Welcome", file.Entries[0].Source);
        Assert.Equal("Willkommen", file.Entries[0].Value);
    }

    [Fact]
    public void Parse_Xliff12WithNote_PreservesComment()
    {
        string xliff = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
              <file source-language="en" target-language="fr" datatype="plaintext">
                <body>
                  <trans-unit id="button.save">
                    <source>Save</source>
                    <target>Enregistrer</target>
                    <note>Button label for saving data</note>
                  </trans-unit>
                </body>
              </file>
            </xliff>
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xliff));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Equal("Button label for saving data", file.Entries[0].Comment);
    }

    [Fact]
    public void Parse_XliffWithoutNamespace_ParsesSuccessfully()
    {
        string xliff = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xliff version="1.2">
              <file source-language="en" target-language="es" datatype="plaintext">
                <body>
                  <trans-unit id="hello">
                    <source>Hello</source>
                    <target>Hola</target>
                  </trans-unit>
                </body>
              </file>
            </xliff>
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xliff));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Equal("Hola", file.GetValue("hello"));
    }

    [Fact]
    public void Parse_MissingTarget_UsesSourceAsValue()
    {
        string xliff = """
            <?xml version="1.0" encoding="UTF-8"?>
            <xliff version="1.2" xmlns="urn:oasis:names:tc:xliff:document:1.2">
              <file source-language="en" datatype="plaintext">
                <body>
                  <trans-unit id="untranslated">
                    <source>Untranslated text</source>
                  </trans-unit>
                </body>
              </file>
            </xliff>
            """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(xliff));
        LocalizationFile file = _format.Parse(stream);

        Assert.Single(file.Entries);
        Assert.Equal("Untranslated text", file.GetValue("untranslated"));
    }

    [Fact]
    public void Write_CreatesValidXliff12Structure()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.xlf",
            Culture = "de",
            Entries =
            [
                new LocalizationEntry { Key = "greeting", Value = "Hallo", Source = "Hello" },
                new LocalizationEntry { Key = "farewell", Value = "Auf Wiedersehen", Source = "Goodbye" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xliff = reader.ReadToEnd();

        Assert.Contains("xliff", xliff);
        Assert.Contains("trans-unit", xliff);
        Assert.Contains("greeting", xliff);
        Assert.Contains("Hallo", xliff);
        Assert.Contains("Hello", xliff);
    }

    [Fact]
    public void Write_IncludesTargetLanguage()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.xlf",
            Culture = "ja",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "こんにちは" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xliff = reader.ReadToEnd();

        Assert.Contains("target-language=\"ja\"", xliff);
    }

    [Fact]
    public void Write_IncludesNote_WhenCommentPresent()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.xlf",
            Entries =
            [
                new LocalizationEntry { Key = "msg", Value = "Message", Comment = "This is a comment" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string xliff = reader.ReadToEnd();

        Assert.Contains("<note>This is a comment</note>", xliff);
    }

    [Fact]
    public void Roundtrip_PreservesData()
    {
        LocalizationFile original = new()
        {
            FilePath = "test.xlf",
            Culture = "fr",
            Entries =
            [
                new LocalizationEntry { Key = "greeting", Value = "Bonjour", Source = "Hello", Comment = "Greeting message" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(original, stream);

        stream.Position = 0;
        LocalizationFile parsed = _format.Parse(stream);

        Assert.Single(parsed.Entries);
        Assert.Equal("greeting", parsed.Entries[0].Key);
        Assert.Equal("Bonjour", parsed.Entries[0].Value);
        Assert.Equal("Hello", parsed.Entries[0].Source);
        Assert.Equal("Greeting message", parsed.Entries[0].Comment);
    }
}