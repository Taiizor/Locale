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
}