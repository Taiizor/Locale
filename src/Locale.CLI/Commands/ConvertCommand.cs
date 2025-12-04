using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the convert command.
/// </summary>
public sealed class ConvertSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the source file or directory to convert.
    /// </summary>
    [Description("Source file or directory.")]
    [CommandArgument(0, "<source>")]
    public required string Source { get; set; }

    /// <summary>
    /// Gets or sets the destination file or directory.
    /// </summary>
    [Description("Destination file or directory.")]
    [CommandArgument(1, "<destination>")]
    public required string Destination { get; set; }

    /// <summary>
    /// Gets or sets the source format (auto-detected if not specified).
    /// </summary>
    [Description("Source format (auto-detected if not specified).")]
    [CommandOption("-f|--from")]
    public string? FromFormat { get; set; }

    /// <summary>
    /// Gets or sets the target format.
    /// </summary>
    [Description("Target format (required).")]
    [CommandOption("-t|--to")]
    public string? ToFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to process directories recursively.
    /// </summary>
    [Description("Process directories recursively.")]
    [CommandOption("-r|--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing files.
    /// </summary>
    [Description("Overwrite existing files.")]
    [CommandOption("--force")]
    public bool Force { get; set; }

    /// <summary>
    /// Gets or sets the culture override for the output file.
    /// </summary>
    [Description("Override culture for the output file.")]
    [CommandOption("-c|--culture")]
    public string? Culture { get; set; }
}

/// <summary>
/// CLI command for converting localization files between different formats.
/// Supports JSON, YAML, RESX, PO, XLIFF, and other formats.
/// </summary>
public sealed class ConvertCommand : Command<ConvertSettings>
{
    /// <summary>
    /// Executes the convert command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if conversion succeeded; 1 if any conversion failed; 2 if source not found or format undetectable.</returns>
    protected override int Execute(CommandContext context, ConvertSettings settings, CancellationToken cancellationToken)
    {
        ConvertService service = new();

        // Determine target format
        string? toFormat = settings.ToFormat;
        if (string.IsNullOrEmpty(toFormat))
        {
            // Try to infer from destination extension
            toFormat = InferFormat(settings.Destination);
            if (string.IsNullOrEmpty(toFormat))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Cannot determine target format. Use --to to specify.");
                return 2;
            }
        }

        ConvertOptions options = new()
        {
            FromFormat = settings.FromFormat,
            ToFormat = toFormat,
            Force = settings.Force,
            Recursive = settings.Recursive,
            Culture = settings.Culture
        };

        if (Directory.Exists(settings.Source))
        {
            // Directory conversion
            AnsiConsole.MarkupLine($"[bold]Converting[/] files in {Markup.Escape(settings.Source)} to {toFormat}...");

            List<ConvertResult> results = service.ConvertDirectory(settings.Source, settings.Destination, options);

            int success = results.Count(r => r.Success);
            int failed = results.Count(r => !r.Success);

            foreach (ConvertResult result in results)
            {
                if (result.Success)
                {
                    AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(result.SourcePath)} → {Markup.Escape(result.DestinationPath)}");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(result.SourcePath)}: {Markup.Escape(result.ErrorMessage ?? "Unknown error")}");
                }
            }

            AnsiConsole.MarkupLine($"\n[bold]Summary:[/] {success} converted, {failed} failed");

            return failed > 0 ? 1 : 0;
        }
        else if (File.Exists(settings.Source))
        {
            // Single file conversion
            AnsiConsole.MarkupLine($"[bold]Converting[/] {Markup.Escape(settings.Source)} to {toFormat}...");

            ConvertResult result = service.Convert(settings.Source, settings.Destination, options);

            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(result.SourcePath)} → {Markup.Escape(result.DestinationPath)}");
                return 0;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(result.ErrorMessage ?? "Unknown error")}");
                return 1;
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source not found: {Markup.Escape(settings.Source)}");
            return 2;
        }
    }

    private static string? InferFormat(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".json" => "json",
            ".yaml" or ".yml" => "yaml",
            ".resx" => "resx",
            ".po" => "po",
            ".xlf" or ".xliff" => "xliff",
            _ => null
        };
    }
}