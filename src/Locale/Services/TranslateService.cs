using Locale.Formats;
using Locale.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Locale.Services;

/// <summary>
/// Supported translation providers.
/// </summary>
public enum TranslationProvider
{
    /// <summary>Google Translate (free tier via unofficial API)</summary>
    Google,
    /// <summary>Microsoft Bing Translator</summary>
    Bing,
    /// <summary>Yandex Translate</summary>
    Yandex,
    /// <summary>DeepL Translator</summary>
    DeepL,
    /// <summary>LibreTranslate (open source, self-hostable)</summary>
    LibreTranslate,
    /// <summary>OpenAI ChatGPT (GPT-4, GPT-3.5-turbo)</summary>
    OpenAI,
    /// <summary>Anthropic Claude (Claude 3)</summary>
    Claude,
    /// <summary>Google Gemini AI</summary>
    Gemini,
    /// <summary>Azure OpenAI Service</summary>
    AzureOpenAI,
    /// <summary>Ollama (local LLM)</summary>
    Ollama
}

/// <summary>
/// Options for the translate operation.
/// </summary>
public sealed class TranslateOptions
{
    /// <summary>
    /// Gets or sets the translation provider to use.
    /// </summary>
    public TranslationProvider Provider { get; set; } = TranslationProvider.Google;

    /// <summary>
    /// Gets or sets the API key for the translation service (if required).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint URL (for self-hosted services like LibreTranslate).
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the source language code.
    /// </summary>
    public required string SourceLanguage { get; set; }

    /// <summary>
    /// Gets or sets the target language code.
    /// </summary>
    public required string TargetLanguage { get; set; }

    /// <summary>
    /// Gets or sets whether to overwrite existing translations.
    /// </summary>
    public bool OverwriteExisting { get; set; }

    /// <summary>
    /// Gets or sets whether to only translate missing keys.
    /// </summary>
    public bool OnlyMissing { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to process directories recursively.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay between API calls in milliseconds (for rate limiting).
    /// </summary>
    public int DelayBetweenCalls { get; set; } = 100;

    /// <summary>
    /// Gets or sets the model name for AI providers (e.g., 'gpt-4', 'claude-3-sonnet', 'gemini-pro').
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the degree of parallelism for concurrent translations.
    /// When set to 1 (default), translations are processed sequentially.
    /// Higher values allow multiple translations to run concurrently.
    /// </summary>
    public int DegreeOfParallelism { get; set; } = 1;
}

/// <summary>
/// Result of a translate operation.
/// </summary>
public sealed class TranslateResult
{
    /// <summary>
    /// Gets or sets the file path that was translated.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the number of keys translated.
    /// </summary>
    public int TranslatedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of keys skipped (already translated).
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of keys that failed to translate.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets any error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets whether the operation was successful.
    /// </summary>
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
}

/// <summary>
/// Service for translating localization files using external translation APIs.
/// </summary>
public sealed class TranslateService(FormatRegistry registry) : IDisposable
{
    private readonly HttpClient _httpClient = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateService"/> class with the default format registry.
    /// </summary>
    public TranslateService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Event raised during translation to report progress.
    /// </summary>
    public event EventHandler<TranslateProgressEventArgs>? OnProgress;

    /// <summary>
    /// Translates localization files from source to target language.
    /// </summary>
    public async Task<List<TranslateResult>> TranslateAsync(string inputPath, string outputPath, TranslateOptions options, CancellationToken cancellationToken = default)
    {
        List<TranslateResult> results = [];
        SearchOption searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<string> sourceFiles;

        if (File.Exists(inputPath))
        {
            sourceFiles = [inputPath];
        }
        else if (Directory.Exists(inputPath))
        {
            sourceFiles = Directory.EnumerateFiles(inputPath, "*.*", searchOption)
                .Where(registry.IsSupported);
        }
        else
        {
            results.Add(new TranslateResult
            {
                FilePath = inputPath,
                ErrorMessage = $"Input path does not exist: {inputPath}"
            });
            return results;
        }

        foreach (string sourceFilePath in sourceFiles)
        {
            ILocalizationFormat? format = registry.GetFormatForFile(sourceFilePath);
            if (format == null)
            {
                continue;
            }

            LocalizationFile sourceFile;
            try
            {
                sourceFile = format.Parse(sourceFilePath);
            }
            catch (Exception ex)
            {
                results.Add(new TranslateResult
                {
                    FilePath = sourceFilePath,
                    ErrorMessage = $"Failed to parse: {ex.Message}"
                });
                continue;
            }

            // Check if this is a source language file
            if (sourceFile.Culture?.Equals(options.SourceLanguage, StringComparison.OrdinalIgnoreCase) != true)
            {
                continue;
            }

            string targetFilePath = PathHelper.GenerateTargetPath(sourceFilePath, inputPath, outputPath,
                options.SourceLanguage, options.TargetLanguage);

            TranslateResult result = await TranslateFileAsync(sourceFile, targetFilePath, format, options, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    private async Task<TranslateResult> TranslateFileAsync(LocalizationFile sourceFile, string targetFilePath,
        ILocalizationFormat format, TranslateOptions options, CancellationToken cancellationToken)
    {
        TranslateResult result = new() { FilePath = targetFilePath };

        try
        {
            Dictionary<string, LocalizationEntry> existingEntries = [];

            // Load existing file if it exists
            if (File.Exists(targetFilePath))
            {
                try
                {
                    LocalizationFile existingFile = format.Parse(targetFilePath);
                    foreach (LocalizationEntry entry in existingFile.Entries)
                    {
                        existingEntries[entry.Key] = entry;
                    }
                }
                catch
                {
                    // Ignore parse errors for existing file
                }
            }

            int totalEntries = sourceFile.Entries.Count;
            int processedEntries = 0;

            // Use array to store results with index for ordering
            LocalizationEntry[] translatedEntries = new LocalizationEntry[totalEntries];
            int translatedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            int degreeOfParallelism = Math.Max(1, options.DegreeOfParallelism);

            if (degreeOfParallelism == 1)
            {
                // Sequential processing (original behavior)
                for (int i = 0; i < totalEntries; i++)
                {
                    LocalizationEntry sourceEntry = sourceFile.Entries[i];
                    cancellationToken.ThrowIfCancellationRequested();

                    processedEntries++;
                    OnProgress?.Invoke(this, new TranslateProgressEventArgs(processedEntries, totalEntries, sourceEntry.Key));

                    LocalizationEntry? existingEntry = existingEntries.GetValueOrDefault(sourceEntry.Key);

                    // Check if we should skip this entry
                    if (existingEntry != null && !options.OverwriteExisting && !string.IsNullOrEmpty(existingEntry.Value))
                    {
                        translatedEntries[i] = existingEntry;
                        skippedCount++;
                        continue;
                    }

                    // Only translate if source has a value
                    if (string.IsNullOrEmpty(sourceEntry.Value))
                    {
                        translatedEntries[i] = new LocalizationEntry
                        {
                            Key = sourceEntry.Key,
                            Value = "",
                            Comment = sourceEntry.Comment
                        };
                        skippedCount++;
                        continue;
                    }

                    try
                    {
                        string translatedValue = await TranslateTextAsync(
                            sourceEntry.Value,
                            options.SourceLanguage,
                            options.TargetLanguage,
                            options,
                            cancellationToken);

                        translatedEntries[i] = new LocalizationEntry
                        {
                            Key = sourceEntry.Key,
                            Value = translatedValue,
                            Comment = sourceEntry.Comment,
                            Source = sourceEntry.Value
                        };
                        translatedCount++;

                        // Rate limiting
                        if (options.DelayBetweenCalls > 0)
                        {
                            await Task.Delay(options.DelayBetweenCalls, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Keep original or empty value on failure
                        translatedEntries[i] = new LocalizationEntry
                        {
                            Key = sourceEntry.Key,
                            Value = existingEntry?.Value ?? "",
                            Comment = $"Translation failed: {ex.Message}"
                        };
                        failedCount++;
                    }
                }
            }
            else
            {
                // Parallel processing with rate limiting using SemaphoreSlim
                using SemaphoreSlim semaphore = new(degreeOfParallelism, degreeOfParallelism);

                // Emit initial progress event to initialize the progress bar before parallel execution
                OnProgress?.Invoke(this, new TranslateProgressEventArgs(0, totalEntries, "Starting..."));

                List<Task> tasks = [];

                for (int i = 0; i < totalEntries; i++)
                {
                    int index = i; // Capture for closure
                    LocalizationEntry sourceEntry = sourceFile.Entries[index];

                    Task task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            LocalizationEntry? existingEntry = existingEntries.GetValueOrDefault(sourceEntry.Key);

                            // Check if we should skip this entry
                            if (existingEntry != null && !options.OverwriteExisting && !string.IsNullOrEmpty(existingEntry.Value))
                            {
                                translatedEntries[index] = existingEntry;
                                Interlocked.Increment(ref skippedCount);
                            }
                            // Only translate if source has a value
                            else if (string.IsNullOrEmpty(sourceEntry.Value))
                            {
                                translatedEntries[index] = new LocalizationEntry
                                {
                                    Key = sourceEntry.Key,
                                    Value = "",
                                    Comment = sourceEntry.Comment
                                };
                                Interlocked.Increment(ref skippedCount);
                            }
                            else
                            {
                                try
                                {
                                    string translatedValue = await TranslateTextAsync(
                                        sourceEntry.Value,
                                        options.SourceLanguage,
                                        options.TargetLanguage,
                                        options,
                                        cancellationToken);

                                    translatedEntries[index] = new LocalizationEntry
                                    {
                                        Key = sourceEntry.Key,
                                        Value = translatedValue,
                                        Comment = sourceEntry.Comment,
                                        Source = sourceEntry.Value
                                    };
                                    Interlocked.Increment(ref translatedCount);

                                    // Rate limiting delay inside semaphore to control API call rate
                                    if (options.DelayBetweenCalls > 0)
                                    {
                                        await Task.Delay(options.DelayBetweenCalls, cancellationToken);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Keep original or empty value on failure
                                    translatedEntries[index] = new LocalizationEntry
                                    {
                                        Key = sourceEntry.Key,
                                        Value = existingEntry?.Value ?? "",
                                        Comment = $"Translation failed: {ex.Message}"
                                    };
                                    Interlocked.Increment(ref failedCount);
                                }
                            }

                            // Update progress after processing
                            int currentProcessed = Interlocked.Increment(ref processedEntries);
                            OnProgress?.Invoke(this, new TranslateProgressEventArgs(currentProcessed, totalEntries, sourceEntry.Key));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, cancellationToken);

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);
            }

            result.TranslatedCount = translatedCount;
            result.SkippedCount = skippedCount;
            result.FailedCount = failedCount;

            // Write target file
            LocalizationFile targetFile = new()
            {
                FilePath = targetFilePath,
                Culture = options.TargetLanguage,
                Format = format.FormatId,
                Entries = [.. translatedEntries]
            };

            format.Write(targetFile, targetFilePath);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Translates a single text string.
    /// </summary>
    public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage,
        TranslateOptions options, CancellationToken cancellationToken = default)
    {
        return options.Provider switch
        {
            TranslationProvider.Google => await TranslateWithGoogleAsync(text, sourceLanguage, targetLanguage, cancellationToken),
            TranslationProvider.DeepL => await TranslateWithDeepLAsync(text, sourceLanguage, targetLanguage, options.ApiKey, cancellationToken),
            TranslationProvider.LibreTranslate => await TranslateWithLibreTranslateAsync(text, sourceLanguage, targetLanguage, options.ApiEndpoint, options.ApiKey, cancellationToken),
            TranslationProvider.Yandex => await TranslateWithYandexAsync(text, sourceLanguage, targetLanguage, options.ApiKey, cancellationToken),
            TranslationProvider.Bing => await TranslateWithBingAsync(text, sourceLanguage, targetLanguage, options.ApiKey, cancellationToken),
            TranslationProvider.OpenAI => await TranslateWithOpenAIAsync(text, sourceLanguage, targetLanguage, options.ApiKey, options.Model, cancellationToken),
            TranslationProvider.Claude => await TranslateWithClaudeAsync(text, sourceLanguage, targetLanguage, options.ApiKey, options.Model, cancellationToken),
            TranslationProvider.Gemini => await TranslateWithGeminiAsync(text, sourceLanguage, targetLanguage, options.ApiKey, options.Model, cancellationToken),
            TranslationProvider.AzureOpenAI => await TranslateWithAzureOpenAIAsync(text, sourceLanguage, targetLanguage, options.ApiKey, options.ApiEndpoint, options.Model, cancellationToken),
            TranslationProvider.Ollama => await TranslateWithOllamaAsync(text, sourceLanguage, targetLanguage, options.ApiEndpoint, options.Model, cancellationToken),
            _ => throw new NotSupportedException($"Translation provider not supported: {options.Provider}")
        };
    }

    private async Task<string> TranslateWithGoogleAsync(string text, string sourceLang, string targetLang, CancellationToken cancellationToken)
    {
        // Using the free Google Translate API (unofficial)
        string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={HttpUtility.UrlEncode(text)}";

        string response = await _httpClient.GetStringAsync(url, cancellationToken);

        // Parse the response (it's a nested JSON array)
        using JsonDocument doc = JsonDocument.Parse(response);
        StringBuilder result = new();

        if (doc.RootElement.GetArrayLength() > 0)
        {
            JsonElement translations = doc.RootElement[0];
            foreach (JsonElement translation in translations.EnumerateArray())
            {
                if (translation.GetArrayLength() > 0)
                {
                    result.Append(translation[0].GetString());
                }
            }
        }

        return result.ToString();
    }

    private async Task<string> TranslateWithDeepLAsync(string text, string sourceLang, string targetLang, string? apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("DeepL API key is required");
        }

        string url = "https://api-free.deepl.com/v2/translate";

        FormUrlEncodedContent content = new(
        [
            new KeyValuePair<string, string>("auth_key", apiKey),
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("source_lang", sourceLang.ToUpperInvariant()),
            new KeyValuePair<string, string>("target_lang", targetLang.ToUpperInvariant())
        ]);

        HttpResponseMessage response = await _httpClient.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        DeepLResponse? json = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken: cancellationToken);
        return json?.Translations?.FirstOrDefault()?.Text ?? text;
    }

    private async Task<string> TranslateWithLibreTranslateAsync(string text, string sourceLang, string targetLang, string? endpoint, string? apiKey, CancellationToken cancellationToken)
    {
        string url = string.IsNullOrEmpty(endpoint) ? "https://libretranslate.com/translate" : $"{endpoint.TrimEnd('/')}/translate";

        var requestBody = new
        {
            q = text,
            source = sourceLang,
            target = targetLang,
            api_key = apiKey
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        LibreTranslateResponse? json = await response.Content.ReadFromJsonAsync<LibreTranslateResponse>(cancellationToken: cancellationToken);
        return json?.TranslatedText ?? text;
    }

    private async Task<string> TranslateWithYandexAsync(string text, string sourceLang, string targetLang, string? apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Yandex API key is required");
        }

        string url = $"https://translate.api.cloud.yandex.net/translate/v2/translate";

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Api-Key {apiKey}");

        var requestBody = new
        {
            sourceLanguageCode = sourceLang,
            targetLanguageCode = targetLang,
            texts = new[] { text }
        };

        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        YandexResponse? json = await response.Content.ReadFromJsonAsync<YandexResponse>(cancellationToken: cancellationToken);
        return json?.Translations?.FirstOrDefault()?.Text ?? text;
    }

    private async Task<string> TranslateWithBingAsync(string text, string sourceLang, string targetLang, string? apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Microsoft Translator API key is required");
        }

        string url = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from={sourceLang}&to={targetLang}";

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

        var requestBody = new[] { new { Text = text } };
        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        BingResponse[]? json = await response.Content.ReadFromJsonAsync<BingResponse[]>(cancellationToken: cancellationToken);
        return json?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? text;
    }

    private async Task<string> TranslateWithOpenAIAsync(string text, string sourceLang, string targetLang, string? apiKey, string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is required");
        }

        string url = "https://api.openai.com/v1/chat/completions";
        string modelName = string.IsNullOrEmpty(model) ? "gpt-4o-mini" : model;

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "system", content = $"You are a professional translator. Translate the following text from {sourceLang} to {targetLang}. Only provide the translation, no explanations or additional text." },
                new { role = "user", content = text }
            },
            temperature = 0.3,
            max_tokens = 4096
        };

        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        OpenAIResponse? json = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
        return json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? text;
    }

    private async Task<string> TranslateWithClaudeAsync(string text, string sourceLang, string targetLang, string? apiKey, string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Anthropic API key is required");
        }

        string url = "https://api.anthropic.com/v1/messages";
        string modelName = string.IsNullOrEmpty(model) ? "claude-3-5-sonnet-latest" : model;

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var requestBody = new
        {
            model = modelName,
            max_tokens = 4096,
            system = $"You are a professional translator. Translate text from {sourceLang} to {targetLang}. Only provide the translation, no explanations or additional text.",
            messages = new[]
            {
                new { role = "user", content = text }
            }
        };

        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        ClaudeResponse? json = await response.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken: cancellationToken);
        return json?.Content?.FirstOrDefault()?.Text?.Trim() ?? text;
    }

    private async Task<string> TranslateWithGeminiAsync(string text, string sourceLang, string targetLang, string? apiKey, string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Google Gemini API key is required");
        }

        string modelName = string.IsNullOrEmpty(model) ? "gemini-2.0-flash" : model;
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"Translate the following text from {sourceLang} to {targetLang}. Only provide the translation, no explanations or additional text.\n\n{text}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 4096
            }
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        GeminiResponse? json = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
        return json?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim() ?? text;
    }

    private async Task<string> TranslateWithAzureOpenAIAsync(string text, string sourceLang, string targetLang, string? apiKey, string? endpoint, string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key is required");
        }

        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint is required");
        }

        string deploymentName = string.IsNullOrEmpty(model) ? "gpt-4" : model;
        string url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deploymentName}/chat/completions?api-version=2024-02-15-preview";

        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Add("api-key", apiKey);

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = $"You are a professional translator. Translate the following text from {sourceLang} to {targetLang}. Only provide the translation, no explanations or additional text." },
                new { role = "user", content = text }
            },
            temperature = 0.3,
            max_tokens = 4096
        };

        request.Content = JsonContent.Create(requestBody);

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        OpenAIResponse? json = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: cancellationToken);
        return json?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? text;
    }

    private async Task<string> TranslateWithOllamaAsync(string text, string sourceLang, string targetLang, string? endpoint, string? model, CancellationToken cancellationToken)
    {
        string url = string.IsNullOrEmpty(endpoint) ? "http://localhost:11434/api/generate" : $"{endpoint.TrimEnd('/')}/api/generate";
        string modelName = string.IsNullOrEmpty(model) ? "llama3.2" : model;

        var requestBody = new
        {
            model = modelName,
            prompt = $"You are a professional translator. Translate the following text from {sourceLang} to {targetLang}. Only provide the translation, no explanations or additional text.\n\n{text}",
            stream = false,
            options = new
            {
                temperature = 0.3
            }
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        OllamaResponse? json = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        return json?.Response?.Trim() ?? text;
    }



    /// <summary>
    /// Releases all resources used by the <see cref="TranslateService"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _httpClient.Dispose();
    }

    // Response models for translation APIs
    private sealed class DeepLResponse
    {
        public List<DeepLTranslation>? Translations { get; set; }
    }

    private sealed class DeepLTranslation
    {
        public string? Text { get; set; }
    }

    private sealed class LibreTranslateResponse
    {
        public string? TranslatedText { get; set; }
    }

    private sealed class YandexResponse
    {
        public List<YandexTranslation>? Translations { get; set; }
    }

    private sealed class YandexTranslation
    {
        public string? Text { get; set; }
    }

    private sealed class BingResponse
    {
        public List<BingTranslation>? Translations { get; set; }
    }

    private sealed class BingTranslation
    {
        public string? Text { get; set; }
    }

    // AI Provider Response Models
    private sealed class OpenAIResponse
    {
        public List<OpenAIChoice>? Choices { get; set; }
    }

    private sealed class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    private sealed class OpenAIMessage
    {
        public string? Content { get; set; }
    }

    private sealed class ClaudeResponse
    {
        public List<ClaudeContent>? Content { get; set; }
    }

    private sealed class ClaudeContent
    {
        public string? Text { get; set; }
    }

    private sealed class GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        public string? Text { get; set; }
    }

    private sealed class OllamaResponse
    {
        public string? Response { get; set; }
    }
}

/// <summary>
/// Event arguments for translation progress.
/// </summary>
/// <param name="current">The current number of entries processed.</param>
/// <param name="total">The total number of entries to process.</param>
/// <param name="currentKey">The key currently being translated.</param>
public class TranslateProgressEventArgs(int current, int total, string currentKey) : EventArgs
{
    /// <summary>
    /// Gets the current number of entries that have been processed.
    /// </summary>
    public int Current { get; } = current;

    /// <summary>
    /// Gets the total number of entries to be processed.
    /// </summary>
    public int Total { get; } = total;

    /// <summary>
    /// Gets the localization key that is currently being translated.
    /// </summary>
    public string CurrentKey { get; } = currentKey;

    /// <summary>
    /// Gets the progress percentage as a value between 0 and 100.
    /// </summary>
    public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
}