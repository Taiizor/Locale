using Locale.Models;
using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the scan command.
/// </summary>
public sealed class ScanSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the path to the directory or file to scan.
    /// </summary>
    [Description("Path to the directory or file to scan.")]
    [CommandArgument(0, "<path>")]
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the base culture to compare against (e.g., 'en').
    /// </summary>
    [Description("Base culture to compare against (e.g., 'en').")]
    [CommandOption("-b|--base")]
    public string BaseCulture { get; set; } = "en";

    /// <summary>
    /// Gets or sets the target cultures to compare (comma-separated, e.g., 'tr,de,fr').
    /// </summary>
    [Description("Target cultures to compare (comma-separated, e.g., 'tr,de,fr').")]
    [CommandOption("-t|--targets")]
    public string? Targets { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to scan directories recursively.
    /// </summary>
    [Description("Scan directories recursively.")]
    [CommandOption("-r|--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the output path for the JSON report.
    /// </summary>
    [Description("Output results to a JSON file.")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets the ignore patterns (comma-separated).
    /// </summary>
    [Description("Ignore patterns (comma-separated).")]
    [CommandOption("--ignore")]
    public string? Ignore { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to check for placeholder mismatches.
    /// </summary>
    [Description("Check for placeholder mismatches.")]
    [CommandOption("--check-placeholders")]
    [DefaultValue(true)]
    public bool CheckPlaceholders { get; set; } = true;
}

/// <summary>
/// CLI command for scanning and comparing localization files across cultures.
/// Detects missing keys, orphan keys, empty values, and placeholder mismatches.
/// </summary>
public sealed class ScanCommand : Command<ScanSettings>
{
    /// <summary>
    /// Executes the scan command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if no issues found; 1 if issues were found.</returns>
    protected override int Execute(CommandContext context, ScanSettings settings, CancellationToken cancellationToken)
    {
        ScanService service = new();

        ScanOptions options = new()
        {
            BaseCulture = settings.BaseCulture,
            TargetCultures = ParseList(settings.Targets),
            Recursive = settings.Recursive,
            IgnorePatterns = ParseList(settings.Ignore),
            CheckPlaceholders = settings.CheckPlaceholders
        };

        AnsiConsole.MarkupLine($"[bold]Scanning[/] {settings.Path}...");
        AnsiConsole.MarkupLine($"[dim]Base culture:[/] {settings.BaseCulture}");

        ScanReport report = service.Scan(settings.Path, options);

        if (!report.HasIssues)
        {
            AnsiConsole.MarkupLine("[green]✓ No issues found![/]");
        }
        else
        {
            RenderReport(report);
        }

        if (!string.IsNullOrEmpty(settings.Output))
        {
            string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settings.Output, json);
            AnsiConsole.MarkupLine($"[dim]Report written to:[/] {settings.Output}");
        }

        return report.HasIssues ? 1 : 0;
    }

    private static void RenderReport(ScanReport report)
    {
        foreach (CultureComparisonResult result in report.Results)
        {
            AnsiConsole.MarkupLine($"\n[bold yellow]{result.Culture}[/]");

            if (result.MissingKeys.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [red]Missing keys ({result.MissingKeys.Count}):[/]");
                foreach (string? key in result.MissingKeys.Take(10))
                {
                    AnsiConsole.MarkupLine($"    - {Markup.Escape(key)}");
                }
                if (result.MissingKeys.Count > 10)
                {
                    AnsiConsole.MarkupLine($"    [dim]... and {result.MissingKeys.Count - 10} more[/]");
                }
            }

            if (result.OrphanKeys.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Orphan keys ({result.OrphanKeys.Count}):[/]");
                foreach (string? key in result.OrphanKeys.Take(10))
                {
                    AnsiConsole.MarkupLine($"    - {Markup.Escape(key)}");
                }
                if (result.OrphanKeys.Count > 10)
                {
                    AnsiConsole.MarkupLine($"    [dim]... and {result.OrphanKeys.Count - 10} more[/]");
                }
            }

            if (result.EmptyValues.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Empty values ({result.EmptyValues.Count}):[/]");
                foreach (string? key in result.EmptyValues.Take(10))
                {
                    AnsiConsole.MarkupLine($"    - {Markup.Escape(key)}");
                }
                if (result.EmptyValues.Count > 10)
                {
                    AnsiConsole.MarkupLine($"    [dim]... and {result.EmptyValues.Count - 10} more[/]");
                }
            }

            if (result.PlaceholderMismatches.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [red]Placeholder mismatches ({result.PlaceholderMismatches.Count}):[/]");
                foreach (PlaceholderMismatch? mismatch in result.PlaceholderMismatches.Take(5))
                {
                    AnsiConsole.MarkupLine($"    - {Markup.Escape(mismatch.Key)}");
                }
                if (result.PlaceholderMismatches.Count > 5)
                {
                    AnsiConsole.MarkupLine($"    [dim]... and {result.PlaceholderMismatches.Count - 5} more[/]");
                }
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Summary:[/] {report.TotalMissingKeys} missing, {report.TotalOrphanKeys} orphan, {report.TotalEmptyValues} empty");
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }
}