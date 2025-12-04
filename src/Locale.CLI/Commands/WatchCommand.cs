using Locale.Models;
using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the watch command.
/// </summary>
public sealed class WatchSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the path to the directory to watch.
    /// </summary>
    [Description("Path to the directory to watch.")]
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
    /// Gets or sets the watch mode: 'scan' or 'check'.
    /// </summary>
    [Description("Watch mode: 'scan' or 'check'.")]
    [CommandOption("-m|--mode")]
    [DefaultValue("scan")]
    public string Mode { get; set; } = "scan";

    /// <summary>
    /// Gets or sets a value indicating whether to include subdirectories.
    /// </summary>
    [Description("Include subdirectories.")]
    [CommandOption("-r|--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the debounce interval in milliseconds.
    /// </summary>
    [Description("Debounce interval in milliseconds.")]
    [CommandOption("--debounce")]
    [DefaultValue(500)]
    public int DebounceMs { get; set; } = 500;
}

/// <summary>
/// CLI command for watching localization files for changes and automatically re-running scan or check.
/// </summary>
public sealed class WatchCommand : Command<WatchSettings>
{
    /// <summary>
    /// Executes the watch command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 on normal exit; 1 if directory not found.</returns>
    protected override int Execute(CommandContext context, WatchSettings settings, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(settings.Path))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Directory not found: {settings.Path}");
            return 1;
        }

        WatchOptions options = new()
        {
            BaseCulture = settings.BaseCulture,
            TargetCultures = ParseList(settings.Targets),
            Mode = settings.Mode.Equals("check", StringComparison.OrdinalIgnoreCase)
                ? WatchMode.Check
                : WatchMode.Scan,
            Recursive = settings.Recursive,
            DebounceMs = settings.DebounceMs
        };

        using WatchService service = new();

        service.OnChange += (sender, e) =>
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[dim]{DateTime.Now:HH:mm:ss}[/] File change detected\n");

            if (e.ScanReport != null)
            {
                RenderScanReport(e.ScanReport);
            }
            else if (e.CheckReport != null)
            {
                RenderCheckReport(e.CheckReport);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Watching for changes... (Press Ctrl+C to stop)[/]");
        };

        service.OnError += (sender, e) =>
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {e.Message}");
        };

        AnsiConsole.MarkupLine($"[bold]Watching[/] {settings.Path}");
        AnsiConsole.MarkupLine($"[dim]Mode:[/] {options.Mode}");
        AnsiConsole.MarkupLine($"[dim]Base culture:[/] {settings.BaseCulture}");
        if (options.TargetCultures.Count > 0)
        {
            AnsiConsole.MarkupLine($"[dim]Target cultures:[/] {string.Join(", ", options.TargetCultures)}");
        }
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Watching for changes... (Press Ctrl+C to stop)[/]");

        service.Start(settings.Path, options);

        // Keep running until cancelled
        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            Task.Delay(Timeout.Infinite, cts.Token).Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Normal exit
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Watch stopped.[/]");

        return 0;
    }

    private static void RenderScanReport(Models.ScanReport report)
    {
        if (!report.HasIssues)
        {
            AnsiConsole.MarkupLine("[green]✓ No issues found![/]");
            return;
        }

        foreach (CultureComparisonResult result in report.Results)
        {
            AnsiConsole.MarkupLine($"[bold yellow]{result.Culture}[/]");

            if (result.MissingKeys.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [red]Missing:[/] {result.MissingKeys.Count}");
            }
            if (result.OrphanKeys.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Orphan:[/] {result.OrphanKeys.Count}");
            }
            if (result.EmptyValues.Count > 0)
            {
                AnsiConsole.MarkupLine($"  [yellow]Empty:[/] {result.EmptyValues.Count}");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Total:[/] {report.TotalMissingKeys} missing, {report.TotalOrphanKeys} orphan, {report.TotalEmptyValues} empty");
    }

    private static void RenderCheckReport(Models.CheckReport report)
    {
        if (!report.HasViolations)
        {
            AnsiConsole.MarkupLine("[green]✓ All checks passed![/]");
            return;
        }

        foreach (CheckViolation? violation in report.Violations.Take(10))
        {
            string icon = violation.Severity == Models.ViolationSeverity.Error ? "[red]✗[/]" : "[yellow]![/]";
            AnsiConsole.MarkupLine($"  {icon} {Markup.Escape(violation.FilePath)}: {Markup.Escape(violation.Message)}");
        }

        if (report.Violations.Count > 10)
        {
            AnsiConsole.MarkupLine($"  [dim]... and {report.Violations.Count - 10} more[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Total:[/] {report.ViolationCount} violations");
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