using Locale.Models;
using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the diff command.
/// </summary>
public sealed class DiffSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the first localization file to compare.
    /// </summary>
    [Description("First localization file to compare.")]
    [CommandArgument(0, "<first>")]
    public required string First { get; set; }

    /// <summary>
    /// Gets or sets the second localization file to compare.
    /// </summary>
    [Description("Second localization file to compare.")]
    [CommandArgument(1, "<second>")]
    public required string Second { get; set; }

    /// <summary>
    /// Gets or sets the output path for the JSON report.
    /// </summary>
    [Description("Output results to a JSON file.")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to check for placeholder mismatches.
    /// </summary>
    [Description("Check for placeholder mismatches.")]
    [CommandOption("--check-placeholders")]
    [DefaultValue(true)]
    public bool CheckPlaceholders { get; set; } = true;
}

/// <summary>
/// CLI command for comparing two localization files.
/// Identifies keys that are only in one file, empty values, and placeholder mismatches.
/// </summary>
public sealed class DiffCommand : Command<DiffSettings>
{
    /// <summary>
    /// Executes the diff command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if files are equivalent; 1 if differences were found; 2 if files not found.</returns>
    protected override int Execute(CommandContext context, DiffSettings settings, CancellationToken cancellationToken)
    {
        if (!File.Exists(settings.First))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File not found: {Markup.Escape(settings.First)}");
            return 2;
        }

        if (!File.Exists(settings.Second))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File not found: {Markup.Escape(settings.Second)}");
            return 2;
        }

        DiffService service = new();
        DiffOptions options = new()
        {
            CheckPlaceholders = settings.CheckPlaceholders
        };

        AnsiConsole.MarkupLine($"[bold]Comparing[/] {Markup.Escape(settings.First)} [dim]vs[/] {Markup.Escape(settings.Second)}...\n");

        DiffReport report = service.Diff(settings.First, settings.Second, options);

        if (!report.HasDifferences)
        {
            AnsiConsole.MarkupLine("[green]✓ Files are equivalent![/]");
        }
        else
        {
            RenderReport(report);
        }

        if (!string.IsNullOrEmpty(settings.Output))
        {
            string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settings.Output, json);
            AnsiConsole.MarkupLine($"\n[dim]Report written to:[/] {settings.Output}");
        }

        return report.HasDifferences ? 1 : 0;
    }

    private static void RenderReport(DiffReport report)
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Category")
            .AddColumn("Count");

        table.AddRow("Only in first", report.OnlyInFirst.Count.ToString());
        table.AddRow("Only in second", report.OnlyInSecond.Count.ToString());
        table.AddRow("Empty in second", report.EmptyInSecond.Count.ToString());
        table.AddRow("Placeholder mismatches", report.PlaceholderMismatches.Count.ToString());

        AnsiConsole.Write(table);

        if (report.OnlyInFirst.Count > 0)
        {
            AnsiConsole.MarkupLine($"\n[red]Keys only in first file ({report.OnlyInFirst.Count}):[/]");
            foreach (string? key in report.OnlyInFirst.Take(10))
            {
                AnsiConsole.MarkupLine($"  - {Markup.Escape(key)}");
            }
            if (report.OnlyInFirst.Count > 10)
            {
                AnsiConsole.MarkupLine($"  [dim]... and {report.OnlyInFirst.Count - 10} more[/]");
            }
        }

        if (report.OnlyInSecond.Count > 0)
        {
            AnsiConsole.MarkupLine($"\n[yellow]Keys only in second file ({report.OnlyInSecond.Count}):[/]");
            foreach (string? key in report.OnlyInSecond.Take(10))
            {
                AnsiConsole.MarkupLine($"  - {Markup.Escape(key)}");
            }
            if (report.OnlyInSecond.Count > 10)
            {
                AnsiConsole.MarkupLine($"  [dim]... and {report.OnlyInSecond.Count - 10} more[/]");
            }
        }

        if (report.EmptyInSecond.Count > 0)
        {
            AnsiConsole.MarkupLine($"\n[yellow]Empty values in second file ({report.EmptyInSecond.Count}):[/]");
            foreach (string? key in report.EmptyInSecond.Take(10))
            {
                AnsiConsole.MarkupLine($"  - {Markup.Escape(key)}");
            }
            if (report.EmptyInSecond.Count > 10)
            {
                AnsiConsole.MarkupLine($"  [dim]... and {report.EmptyInSecond.Count - 10} more[/]");
            }
        }
    }
}