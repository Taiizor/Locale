using Locale.Services;

namespace Locale.Tests.Services;

public class PathHelperTests
{
    [Theory]
    [InlineData("en.json", ".json")]
    [InlineData("en.yaml", ".yaml")]
    [InlineData("en.i18n.json", ".i18n.json")]
    [InlineData("common.en.i18n.json", ".i18n.json")]
    [InlineData("messages.EN.I18N.JSON", ".i18n.json")] // Case insensitive
    public void GetExtension_ReturnsCorrectExtension(string fileName, string expectedExtension)
    {
        string result = PathHelper.GetExtension(fileName);
        Assert.Equal(expectedExtension, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("en.json", "en")]
    [InlineData("en.yaml", "en")]
    [InlineData("en.i18n.json", "en")]
    [InlineData("common.en.i18n.json", "common.en")]
    [InlineData("messages.EN.I18N.JSON", "messages.EN")] // Case insensitive for extension
    public void GetFileNameWithoutExtension_ReturnsCorrectName(string fileName, string expectedName)
    {
        string result = PathHelper.GetFileNameWithoutExtension(fileName);
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData("en.json", "en", "tr", "tr.json")]
    [InlineData("en.yaml", "en", "de", "de.yaml")]
    [InlineData("en.i18n.json", "en", "tr", "tr.i18n.json")]
    [InlineData("common.en.json", "en", "tr", "common.tr.json")]
    [InlineData("common.en.i18n.json", "en", "tr", "common.tr.i18n.json")]
    public void GenerateTargetPath_GeneratesCorrectFileName(string sourceFileName, string sourceCulture, string targetCulture, string expectedTargetFileName)
    {
        // Create temp directory for testing
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            string sourceFilePath = Path.Combine(tempDir, sourceFileName);
            File.WriteAllText(sourceFilePath, "test");

            string result = PathHelper.GenerateTargetPath(sourceFilePath, tempDir, tempDir, sourceCulture, targetCulture);

            Assert.Equal(expectedTargetFileName, Path.GetFileName(result));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}