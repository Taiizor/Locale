using Locale.Services;

namespace Locale.Tests.Services;

public class TranslateServiceTests
{
    [Fact]
    public void TranslateService_CanBeCreated()
    {
        using TranslateService service = new();
        Assert.NotNull(service);
    }

    [Fact]
    public void TranslationProvider_HasExpectedValues()
    {
        Assert.Equal(0, (int)TranslationProvider.Google);
        Assert.Equal(1, (int)TranslationProvider.Bing);
        Assert.Equal(2, (int)TranslationProvider.Yandex);
        Assert.Equal(3, (int)TranslationProvider.DeepL);
        Assert.Equal(4, (int)TranslationProvider.LibreTranslate);
    }

    [Fact]
    public async Task TranslateAsync_ReturnsEmptyListForNonExistentPath()
    {
        using TranslateService service = new();
        TranslateOptions options = new()
        {
            SourceLanguage = "en",
            TargetLanguage = "tr"
        };

        List<TranslateResult> results = await service.TranslateAsync("/nonexistent/path", "/output", options);

        Assert.Single(results);
        Assert.False(results[0].Success);
        Assert.Contains("does not exist", results[0].ErrorMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void TranslateOptions_DegreeOfParallelism_IsConfigurable(int degreeOfParallelism)
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "en",
            TargetLanguage = "tr",
            DegreeOfParallelism = degreeOfParallelism
        };

        Assert.Equal(degreeOfParallelism, options.DegreeOfParallelism);
    }

    [Fact]
    public void TranslateOptions_DegreeOfParallelism_DefaultsToOne()
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "en",
            TargetLanguage = "tr"
        };

        Assert.Equal(1, options.DegreeOfParallelism);
    }

    [Fact]
    public void TranslateOptions_BaseLanguage_DefaultsToNull()
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "en",
            TargetLanguage = "tr"
        };

        Assert.Null(options.BaseLanguage);
    }

    [Fact]
    public void TranslateOptions_BaseLanguage_IsConfigurable()
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "de",
            TargetLanguage = "en",
            BaseLanguage = "de"
        };

        Assert.Equal("de", options.BaseLanguage);
    }

    [Fact]
    public async Task TranslateAsync_NeutralFileWithoutBaseLanguage_IsSkipped()
    {
        // Issue #24: a neutral Resources.resx (no culture suffix) is currently
        // skipped when --base is not supplied because the detected culture is
        // null and cannot match the source language.
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_translate_neutral_skip_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Resources.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hallo</value></data>
                </root>
                """);

            using TranslateService service = new();
            TranslateOptions options = new()
            {
                SourceLanguage = "de",
                TargetLanguage = "en"
                // BaseLanguage intentionally not set
            };

            List<TranslateResult> results = await service.TranslateAsync(tempDir, tempDir, options, TestContext.Current.CancellationToken);

            // No file matches "de" because the neutral file's detected culture is null.
            Assert.Empty(results);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}