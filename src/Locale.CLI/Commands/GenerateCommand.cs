using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the generate command.
/// </summary>
public sealed class GenerateSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the target culture to generate files for (e.g., 'tr', 'de', 'fr').
    /// </summary>
    [Description("Target culture to generate files for (e.g., 'tr', 'de', 'fr').")]
    [CommandArgument(0, "<target>")]
    public required string Target { get; set; }

    /// <summary>
    /// Gets or sets the base culture to generate from.
    /// </summary>
    [Description("Base culture to generate from.")]
    [CommandOption("-f|--from")]
    [DefaultValue("en")]
    public string From { get; set; } = "en";

    /// <summary>
    /// Gets or sets the input directory or file.
    /// </summary>
    [Description("Input directory or file.")]
    [CommandOption("-i|--in")]
    [DefaultValue(".")]
    public string Input { get; set; } = ".";

    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    [Description("Output directory.")]
    [CommandOption("-o|--out")]
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to process directories recursively.
    /// </summary>
    [Description("Process directories recursively.")]
    [CommandOption("-r|--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use empty values instead of placeholders.
    /// </summary>
    [Description("Use empty values instead of placeholders.")]
    [CommandOption("--empty")]
    public bool UseEmpty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing keys.
    /// </summary>
    [Description("Overwrite existing keys.")]
    [CommandOption("--overwrite")]
    public bool Overwrite { get; set; }

    /// <summary>
    /// Gets or sets the custom placeholder pattern (use {0} for base value).
    /// </summary>
    [Description("Custom placeholder pattern (use {0} for base value).")]
    [CommandOption("--placeholder")]
    [DefaultValue("@@MISSING@@ {0}")]
    public string Placeholder { get; set; } = "@@MISSING@@ {0}";
}

/// <summary>
/// CLI command for generating skeleton target localization files from a base language.
/// Creates missing target files and fills them with placeholder values.
/// </summary>
public sealed class GenerateCommand : Command<GenerateSettings>
{
    /// <summary>
    /// Executes the generate command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if generation succeeded; 1 if any generation failed.</returns>
    protected override int Execute(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        GenerateService service = new();

        GenerateOptions options = new()
        {
            TargetCulture = settings.Target,
            BaseCulture = settings.From,
            Recursive = settings.Recursive,
            UseEmptyValue = settings.UseEmpty,
            OverwriteExisting = settings.Overwrite,
            PlaceholderPattern = settings.Placeholder
        };

        string outputPath = settings.Output ?? settings.Input;

        AnsiConsole.MarkupLine($"[bold]Generating[/] {settings.Target} files from {settings.From}...");
        AnsiConsole.MarkupLine($"[dim]Input:[/] {settings.Input}");
        AnsiConsole.MarkupLine($"[dim]Output:[/] {outputPath}");

        List<GenerateResult> results = service.Generate(settings.Input, outputPath, options);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No base culture files found.[/]");
            return 0;
        }

        int created = 0;
        int updated = 0;
        int failed = 0;

        foreach (GenerateResult result in results)
        {
            if (!result.Success)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(result.FilePath)}: {Markup.Escape(result.ErrorMessage ?? "Unknown error")}");
                failed++;
            }
            else if (result.Created)
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Created: {Markup.Escape(result.FilePath)} ({result.KeysAdded} keys)");
                created++;
            }
            else
            {
                AnsiConsole.MarkupLine($"[blue]✓[/] Updated: {Markup.Escape(result.FilePath)} (+{result.KeysAdded}, ={result.KeysSkipped})");
                updated++;
            }
        }

        AnsiConsole.MarkupLine($"\n[bold]Summary:[/] {created} created, {updated} updated, {failed} failed");

        return failed > 0 ? 1 : 0;
    }
}