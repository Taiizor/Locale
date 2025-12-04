using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the translate command.
/// </summary>
public sealed class TranslateSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the target language to translate to (e.g., 'tr', 'de', 'fr').
    /// </summary>
    [Description("Target language to translate to (e.g., 'tr', 'de', 'fr').")]
    [CommandArgument(0, "<target>")]
    public required string Target { get; set; }

    /// <summary>
    /// Gets or sets the source language to translate from.
    /// </summary>
    [Description("Source language to translate from.")]
    [CommandOption("-f|--from")]
    public string SourceLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets the input directory or file.
    /// </summary>
    [Description("Input directory or file.")]
    [CommandOption("-i|--in")]
    public string InputPath { get; set; } = ".";

    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    [Description("Output directory.")]
    [CommandOption("-o|--out")]
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the translation provider: google, deepl, bing, yandex, libretranslate, openai, claude, gemini, azure-openai, ollama.
    /// </summary>
    [Description("Translation provider: google, deepl, bing, yandex, libretranslate, openai, claude, gemini, azure-openai, ollama.")]
    [CommandOption("-p|--provider")]
    [DefaultValue("google")]
    public string Provider { get; set; } = "google";

    /// <summary>
    /// Gets or sets the API key for the translation service (required for most providers).
    /// </summary>
    [Description("API key for the translation service (required for most providers).")]
    [CommandOption("-k|--api-key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint URL (for self-hosted services like LibreTranslate, Ollama, or Azure OpenAI).
    /// </summary>
    [Description("API endpoint URL (for self-hosted services like LibreTranslate, Ollama, or Azure OpenAI).")]
    [CommandOption("--endpoint")]
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the model name for AI providers (e.g., 'gpt-4o', 'claude-3-5-sonnet-latest', 'gemini-2.0-flash').
    /// </summary>
    [Description("Model name for AI providers (e.g., 'gpt-4o', 'claude-3-5-sonnet-latest', 'gemini-2.0-flash').")]
    [CommandOption("-m|--model")]
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to process directories recursively.
    /// </summary>
    [Description("Process directories recursively.")]
    [CommandOption("-r|--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing translations.
    /// </summary>
    [Description("Overwrite existing translations.")]
    [CommandOption("--overwrite")]
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to only translate missing keys.
    /// </summary>
    [Description("Only translate missing keys.")]
    [CommandOption("--only-missing")]
    [DefaultValue(true)]
    public bool OnlyMissing { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay between API calls in milliseconds (for rate limiting).
    /// </summary>
    [Description("Delay between API calls in milliseconds (for rate limiting).")]
    [CommandOption("--delay")]
    [DefaultValue(100)]
    public int Delay { get; set; } = 100;

    /// <summary>
    /// Gets or sets the degree of parallelism for concurrent translations.
    /// </summary>
    [Description("Degree of parallelism for concurrent translations (1 = sequential, higher = faster but may hit rate limits).")]
    [CommandOption("--parallel")]
    [DefaultValue(1)]
    public int DegreeOfParallelism { get; set; } = 1;
}

/// <summary>
/// CLI command for automatically translating localization files using external APIs.
/// Supports traditional translation services (Google, DeepL, Bing, Yandex, LibreTranslate)
/// and AI-powered providers (OpenAI, Claude, Gemini, Azure OpenAI, Ollama).
/// </summary>
public sealed class TranslateCommand : AsyncCommand<TranslateSettings>
{
    /// <summary>
    /// Executes the translate command asynchronously with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if all translations succeeded; 1 if any translation failed or API key/endpoint missing.</returns>
    protected override async Task<int> ExecuteAsync(CommandContext context, TranslateSettings settings, CancellationToken cancellationToken)
    {
        TranslationProvider provider = ParseProvider(settings.Provider);

        // Validate API key for providers that require it
        bool requiresApiKey = provider switch
        {
            TranslationProvider.Google => false,
            TranslationProvider.Ollama => false,
            TranslationProvider.LibreTranslate => false,
            _ => true
        };

        if (requiresApiKey && string.IsNullOrEmpty(settings.ApiKey))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] API key is required for {provider}. Use --api-key to provide it.");
            return 1;
        }

        // Validate endpoint for providers that require it
        if (provider == TranslationProvider.AzureOpenAI && string.IsNullOrEmpty(settings.ApiEndpoint))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] API endpoint is required for Azure OpenAI. Use --endpoint to provide it.");
            return 1;
        }

        string outputPath = settings.OutputPath ?? settings.InputPath;

        TranslateOptions options = new()
        {
            Provider = provider,
            ApiKey = settings.ApiKey,
            ApiEndpoint = settings.ApiEndpoint,
            SourceLanguage = settings.SourceLanguage,
            TargetLanguage = settings.Target,
            OverwriteExisting = settings.Overwrite,
            OnlyMissing = settings.OnlyMissing,
            Recursive = settings.Recursive,
            DelayBetweenCalls = settings.Delay,
            Model = settings.Model,
            DegreeOfParallelism = settings.DegreeOfParallelism
        };

        AnsiConsole.MarkupLine($"[bold]Translating[/] from [cyan]{settings.SourceLanguage}[/] to [cyan]{settings.Target}[/]");
        AnsiConsole.MarkupLine($"[dim]Provider:[/] {provider}");
        if (!string.IsNullOrEmpty(settings.Model))
        {
            AnsiConsole.MarkupLine($"[dim]Model:[/] {settings.Model}");
        }
        if (settings.DegreeOfParallelism > 1)
        {
            AnsiConsole.MarkupLine($"[dim]Parallelism:[/] {settings.DegreeOfParallelism}");
        }
        AnsiConsole.MarkupLine($"[dim]Input:[/] {settings.InputPath}");
        AnsiConsole.MarkupLine($"[dim]Output:[/] {outputPath}");
        AnsiConsole.WriteLine();

        using TranslateService service = new();
        using CancellationTokenSource cts = new();

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            AnsiConsole.MarkupLine("\n[yellow]Cancelling...[/]");
        };

        Task<List<TranslateResult>> progressTask = AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                ProgressTask? currentTask = null;

                service.OnProgress += (sender, e) =>
                {
                    currentTask ??= ctx.AddTask($"Translating...", maxValue: e.Total);
                    currentTask.Value = e.Current;
                    currentTask.Description = $"Translating: {Truncate(e.CurrentKey, 30)}";
                };

                try
                {
                    List<TranslateResult> results = await service.TranslateAsync(settings.InputPath, outputPath, options, cts.Token);
                    return results;
                }
                catch (OperationCanceledException)
                {
                    return [];
                }
            });

        List<TranslateResult> results = await progressTask;

        AnsiConsole.WriteLine();

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No files found to translate.[/]");
            return 0;
        }

        // Show results
        Table table = new Table()
            .AddColumn("File")
            .AddColumn("Translated")
            .AddColumn("Skipped")
            .AddColumn("Failed")
            .AddColumn("Status");

        foreach (TranslateResult? result in results)
        {
            string status = result.Success ? "[green]✓[/]" : $"[red]✗ {Markup.Escape(result.ErrorMessage ?? "Error")}[/]";
            table.AddRow(
                Markup.Escape(Path.GetFileName(result.FilePath)),
                result.TranslatedCount.ToString(),
                result.SkippedCount.ToString(),
                result.FailedCount.ToString(),
                status);
        }

        AnsiConsole.Write(table);

        int totalTranslated = results.Sum(r => r.TranslatedCount);
        int totalSkipped = results.Sum(r => r.SkippedCount);
        int totalFailed = results.Sum(r => r.FailedCount);
        int successCount = results.Count(r => r.Success);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Summary:[/] {successCount}/{results.Count} files processed, {totalTranslated} translated, {totalSkipped} skipped, {totalFailed} failed");

        return results.All(r => r.Success) ? 0 : 1;
    }

    private static TranslationProvider ParseProvider(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "google" => TranslationProvider.Google,
            "deepl" => TranslationProvider.DeepL,
            "bing" or "microsoft" => TranslationProvider.Bing,
            "yandex" => TranslationProvider.Yandex,
            "libretranslate" or "libre" => TranslationProvider.LibreTranslate,
            "openai" or "chatgpt" or "gpt" => TranslationProvider.OpenAI,
            "claude" or "anthropic" => TranslationProvider.Claude,
            "gemini" or "google-ai" => TranslationProvider.Gemini,
            "azure-openai" or "azure" or "azureopenai" => TranslationProvider.AzureOpenAI,
            "ollama" or "local" => TranslationProvider.Ollama,
            _ => TranslationProvider.Google
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..(maxLength - 3)] + "...";
    }
}