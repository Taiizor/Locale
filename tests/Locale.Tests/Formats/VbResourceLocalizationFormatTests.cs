using Locale.Formats;
using Locale.Models;

namespace Locale.Tests.Formats;

public class VbResourceLocalizationFormatTests
{
    private readonly VbResourceLocalizationFormat _format = new();

    [Fact]
    public void FormatId_ReturnsVb()
    {
        Assert.Equal("vb", _format.FormatId);
    }

    [Fact]
    public void CanHandle_VbFile_ReturnsTrue()
    {
        Assert.True(_format.CanHandle("test.vb"));
        Assert.True(_format.CanHandle("Resources.Designer.vb"));
    }

    [Fact]
    public void CanHandle_NonVbFile_ReturnsFalse()
    {
        Assert.False(_format.CanHandle("test.cs"));
        Assert.False(_format.CanHandle("test.resx"));
    }

    [Fact]
    public void Parse_ResourceProperty_ExtractsKey()
    {
        string vb = """
        Namespace My.Resources
            Friend Module Resources
                Public ReadOnly Property Hello() As String
                    Get
                        Return ResourceManager.GetString("Hello", resourceCulture)
                    End Get
                End Property
            End Module
        End Namespace
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vb));
        LocalizationFile file = _format.Parse(stream, "Resources.vb");

        Assert.Single(file.Entries);
        Assert.Equal("Hello", file.Entries[0].Key);
    }

    [Fact]
    public void Parse_StringConstant_ExtractsKeyAndValue()
    {
        string vb = """
        Friend Const WelcomeMessage As String = "Welcome to our application"
        Public Const Greeting As String = "Hello, World!"
        """;

        using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(vb));
        LocalizationFile file = _format.Parse(stream);

        Assert.Equal(2, file.Entries.Count);
        Assert.Equal("WelcomeMessage", file.Entries[0].Key);
        Assert.Equal("Welcome to our application", file.Entries[0].Value);
        Assert.Equal("Greeting", file.Entries[1].Key);
        Assert.Equal("Hello, World!", file.Entries[1].Value);
    }

    [Fact]
    public void Write_GeneratesWarningHeader()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.vb",
            Entries =
            [
                new LocalizationEntry { Key = "Hello", Value = "Hello World" }
            ]
        };

        using MemoryStream stream = new();
        _format.Write(file, stream);

        stream.Position = 0;
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();

        Assert.Contains("WARNING", content);
        Assert.Contains("auto-generated", content);
        Assert.Contains("Hello", content);
    }
}