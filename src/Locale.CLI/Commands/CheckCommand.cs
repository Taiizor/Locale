using Locale.Models;
using Locale.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Text.Json;

namespace Locale.CLI.Commands;

/// <summary>
/// Command settings for the check command.
/// </summary>
public sealed class CheckSettings : CommandSettings
{
    /// <summary>
    /// Gets or sets the path to the directory or file to check.
    /// </summary>
    [Description("Path to the directory or file to check.")]
    [CommandArgument(0, "<path>")]
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the rules to check (comma-separated).
    /// Available: no-empty-values, no-duplicate-keys, no-orphan-keys, consistent-placeholders, no-trailing-whitespace.
    /// </summary>
    [Description("Rules to check (comma-separated). Available: no-empty-values, no-duplicate-keys, no-orphan-keys, consistent-placeholders, no-trailing-whitespace.")]
    [CommandOption("-r|--rules")]
    public string? Rules { get; set; }

    /// <summary>
    /// Gets or sets the base culture for orphan key and placeholder checks.
    /// </summary>
    [Description("Base culture for orphan key and placeholder checks.")]
    [CommandOption("-b|--base")]
    public string? BaseCulture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to check directories recursively.
    /// </summary>
    [Description("Check directories recursively.")]
    [CommandOption("--recursive")]
    [DefaultValue(true)]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether CI mode is enabled.
    /// When enabled, returns non-zero exit code on violations.
    /// </summary>
    [Description("CI mode: return non-zero exit code on violations.")]
    [CommandOption("--ci")]
    public bool CiMode { get; set; }

    /// <summary>
    /// Gets or sets the output path for the JSON report.
    /// </summary>
    [Description("Output results to a JSON file.")]
    [CommandOption("-o|--output")]
    public string? Output { get; set; }
}

/// <summary>
/// CLI command for validating localization files against configurable rules.
/// Checks for empty values, duplicate keys, orphan keys, placeholder mismatches, and trailing whitespace.
/// </summary>
public sealed class CheckCommand : Command<CheckSettings>
{
    /// <summary>
    /// Executes the check command with the specified settings.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>0 if no violations or CI mode disabled; 1 if violations found in CI mode.</returns>
    protected override int Execute(CommandContext context, CheckSettings settings, CancellationToken cancellationToken)
    {
        CheckService service = new();

        CheckOptions options = new()
        {
            Rules = ParseList(settings.Rules),
            BaseCulture = settings.BaseCulture,
            Recursive = settings.Recursive
        };

        AnsiConsole.MarkupLine($"[bold]Checking[/] {Markup.Escape(settings.Path)}...");

        if (options.Rules.Count > 0)
        {
            AnsiConsole.MarkupLine($"[dim]Rules:[/] {string.Join(", ", options.Rules)}");
        }
        else
        {
            AnsiConsole.MarkupLine($"[dim]Rules:[/] all");
        }

        CheckReport report = service.Check(settings.Path, options);

        if (!report.HasViolations)
        {
            AnsiConsole.MarkupLine("[green]✓ No violations found![/]");
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

        // Return non-zero only in CI mode with violations
        if (settings.CiMode && report.HasViolations)
        {
            return 1;
        }

        return 0;
    }

    private static void RenderReport(Models.CheckReport report)
    {
        AnsiConsole.WriteLine();

        // Group violations by file
        IEnumerable<IGrouping<string, CheckViolation>> byFile = report.Violations.GroupBy(v => v.FilePath);

        foreach (IGrouping<string, CheckViolation> fileGroup in byFile)
        {
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(fileGroup.Key)}[/]");

            foreach (CheckViolation? violation in fileGroup.Take(20))
            {
                string severityColor = violation.Severity switch
                {
                    ViolationSeverity.Error => "red",
                    ViolationSeverity.Warning => "yellow",
                    _ => "dim"
                };

                string keyInfo = !string.IsNullOrEmpty(violation.Key) ? $"[{Markup.Escape(violation.Key)}] " : "";
                AnsiConsole.MarkupLine($"  [{severityColor}]{violation.RuleName}[/]: {keyInfo}{Markup.Escape(violation.Message)}");
            }

            if (fileGroup.Count() > 20)
            {
                AnsiConsole.MarkupLine($"  [dim]... and {fileGroup.Count() - 20} more violations[/]");
            }

            AnsiConsole.WriteLine();
        }

        // Summary by severity
        int errors = report.Violations.Count(v => v.Severity == ViolationSeverity.Error);
        int warnings = report.Violations.Count(v => v.Severity == ViolationSeverity.Warning);
        int infos = report.Violations.Count(v => v.Severity == ViolationSeverity.Info);

        AnsiConsole.MarkupLine($"[bold]Summary:[/] [red]{errors} errors[/], [yellow]{warnings} warnings[/], [dim]{infos} info[/]");
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