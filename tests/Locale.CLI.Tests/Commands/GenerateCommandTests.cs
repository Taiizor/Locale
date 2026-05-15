using Locale.CLI.Commands;

namespace Locale.CLI.Tests.Commands;

public class GenerateCommandTests
{
    [Fact]
    public void GenerateSettings_Properties_ShouldSetCorrectly()
    {
        // Arrange & Act
        GenerateSettings settings = new()
        {
            Target = "tr",
            Input = "/input",
            Output = "/output",
            From = "en",
            Placeholder = "MISSING: {0}",
            UseEmpty = true,
            Overwrite = true,
            Recursive = false
        };

        // Assert
        Assert.Equal("tr", settings.Target);
        Assert.Equal("/input", settings.Input);
        Assert.Equal("/output", settings.Output);
        Assert.Equal("en", settings.From);
        Assert.Equal("MISSING: {0}", settings.Placeholder);
        Assert.True(settings.UseEmpty);
        Assert.True(settings.Overwrite);
        Assert.False(settings.Recursive);
    }

    [Fact]
    public void GenerateSettings_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        GenerateSettings settings = new()
        {
            Target = "tr"
        };

        // Assert
        // From and BaseLanguage default to null; Execute resolves to "en" when neither is set.
        Assert.Null(settings.From);
        Assert.Null(settings.BaseLanguage);
        Assert.Equal(".", settings.Input);
        Assert.Equal("@@MISSING@@ {0}", settings.Placeholder);
        Assert.False(settings.UseEmpty);
        Assert.False(settings.Overwrite);
        Assert.True(settings.Recursive);
    }

    [Fact]
    public void GenerateSettings_BaseLanguage_IsConfigurable()
    {
        // Arrange & Act
        GenerateSettings settings = new()
        {
            Target = "en",
            BaseLanguage = "de"
        };

        // Assert
        Assert.Equal("de", settings.BaseLanguage);
        Assert.Null(settings.From);
    }
}