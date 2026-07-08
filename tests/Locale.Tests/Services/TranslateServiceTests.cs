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

        List<TranslateResult> results = await service.TranslateAsync(
            "/nonexistent/path", "/output", options, TestContext.Current.CancellationToken);

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

    [Fact]
    public void TranslateOptions_CustomParameters_DefaultsToNull()
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "en",
            TargetLanguage = "tr"
        };

        Assert.Null(options.CustomParameters);
    }

    [Fact]
    public void TranslateOptions_CustomParameters_IsConfigurable()
    {
        TranslateOptions options = new()
        {
            SourceLanguage = "de",
            TargetLanguage = "en",
            CustomParameters = "{\"temperature\": 0.5}"
        };

        Assert.Equal("{\"temperature\": 0.5}", options.CustomParameters);
    }

    [Fact]
    public void ApplyCustomParameters_WithValidJson_AppliesParameters()
    {
        Dictionary<string, object> requestBody = new()
        {
            ["existing"] = "value"
        };
        string customParams = "{\"temperature\": 0.7, \"max_tokens\": 1500}";

        TranslateService.ApplyCustomParameters(requestBody, customParams);

        Assert.Equal(3, requestBody.Count);
        Assert.Equal("value", requestBody["existing"]);
        Assert.True(requestBody.ContainsKey("temperature"));
        Assert.True(requestBody.ContainsKey("max_tokens"));
    }

    [Fact]
    public void ApplyCustomParameters_WithSingleQuotes_AppliesParameters()
    {
        Dictionary<string, object> requestBody = [];
        // CLI often strips double quotes and leaves single quotes if users type: --custom-params "{'temperature': 0.8}"
        string customParams = "{'temperature': 0.8, 'model': 'gpt-4'}";

        TranslateService.ApplyCustomParameters(requestBody, customParams);

        Assert.Equal(2, requestBody.Count);
        Assert.True(requestBody.ContainsKey("temperature"));
        Assert.True(requestBody.ContainsKey("model"));
    }

    [Fact]
    public void ApplyCustomParameters_WithTrailingCommas_AppliesParameters()
    {
        Dictionary<string, object> requestBody = [];
        string customParams = "{\"temperature\": 0.9, }";

        TranslateService.ApplyCustomParameters(requestBody, customParams);

        Assert.Single(requestBody);
        Assert.True(requestBody.ContainsKey("temperature"));
    }

    [Fact]
    public void ApplyCustomParameters_WithInvalidJson_DoesNotThrowAndDoesNotApply()
    {
        Dictionary<string, object> requestBody = new()
        {
            ["existing"] = "value"
        };
        string customParams = "{ invalid json ;;; }";

        TranslateService.ApplyCustomParameters(requestBody, customParams);

        Assert.Single(requestBody);
        Assert.Equal("value", requestBody["existing"]);
    }

    [Fact]
    public void ApplyCustomParameters_WithNullOrEmpty_DoesNothing()
    {
        Dictionary<string, object> requestBody = new()
        {
            ["existing"] = "value"
        };

        TranslateService.ApplyCustomParameters(requestBody, null);
        Assert.Single(requestBody);

        TranslateService.ApplyCustomParameters(requestBody, "   ");
        Assert.Single(requestBody);
    }
}