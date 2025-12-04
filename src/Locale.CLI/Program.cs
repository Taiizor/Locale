using Locale.CLI.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using System.Globalization;
using System.Reflection;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

Assembly assembly = Assembly.GetExecutingAssembly();
string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
              ?? assembly.GetName().Version?.ToString()
              ?? "Unknown";

// Header section with project branding
// Using distinct colors from HelpProviderStyles to avoid visual conflict:
// - Magenta for banner/branding (vs Cyan for help headers)
// - DarkOrange for tagline (vs Blue for commands)
// - White for version (vs Green for values)
AnsiConsole.Write(new Rule().RuleStyle(new Style(foreground: Color.Magenta1)));
AnsiConsole.Write(new FigletText("Locale CLI")
    .LeftJustified()
    .Color(Color.DeepPink1));
AnsiConsole.MarkupLine($"[bold darkorange]Multi-format localization toolkit[/] [dim]•[/] [orangered1]v{version}[/]");
AnsiConsole.Write(new Rule().RuleStyle(new Style(foreground: Color.Magenta1)));
AnsiConsole.WriteLine();

CommandApp app = new();

app.Configure(config =>
{
    config.ValidateExamples();

    config.SetApplicationName("locale");
    config.SetApplicationVersion(version);
    config.ConfigureConsole(AnsiConsole.Console);
    config.SetApplicationCulture(CultureInfo.InvariantCulture);

    config.Settings.TrimTrailingPeriod = true;
    config.Settings.MaximumIndirectExamples = 10;
    config.Settings.ShowOptionDefaultValues = true;

    // Professional CLI color scheme with consistent hierarchy:
    // - Headers: Bold Cyan for section titles
    // - Commands/Actions: Blue for executable commands
    // - Required elements: Yellow to indicate importance
    // - Optional elements: Grey for de-emphasized content
    // - Values/Arguments: Green for user-provided data
    config.Settings.HelpProviderStyles = new HelpProviderStyle
    {
        Usage = new UsageStyle
        {
            Command = new Style(foreground: Color.Blue),
            Options = new Style(foreground: Color.Grey),
            RequiredArgument = new Style(foreground: Color.Yellow),
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold),
            CurrentCommand = new Style(foreground: Color.Blue, decoration: Decoration.Bold),
            OptionalArgument = new Style(foreground: Color.Grey, decoration: Decoration.Italic)
        },
        Options = new OptionStyle
        {
            RequiredOption = new Style(foreground: Color.Yellow),
            DefaultValueHeader = new Style(foreground: Color.Grey),
            RequiredOptionValue = new Style(foreground: Color.Green),
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold),
            DefaultValue = new Style(foreground: Color.Grey, decoration: Decoration.Dim),
            OptionalOptionValue = new Style(foreground: Color.Grey, decoration: Decoration.Italic)
        },
        Commands = new CommandStyle
        {
            ChildCommand = new Style(foreground: Color.Blue),
            RequiredArgument = new Style(foreground: Color.Yellow),
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold)
        },
        Examples = new ExampleStyle
        {
            Arguments = new Style(foreground: Color.Green),
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold)
        },
        Arguments = new ArgumentStyle
        {
            RequiredArgument = new Style(foreground: Color.Yellow),
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold),
            OptionalArgument = new Style(foreground: Color.Grey, decoration: Decoration.Italic)
        },
        Description = new DescriptionStyle
        {
            Header = new Style(foreground: Color.Cyan, decoration: Decoration.Bold)
        }
    };

    config.AddCommand<ScanCommand>("scan")
        .WithDescription("Scan and compare localization files across cultures.")
        .WithExample("scan", "./locales", "--base", "en", "--targets", "tr,de");

    config.AddCommand<DiffCommand>("diff")
        .WithDescription("Compare two localization files.")
        .WithExample("diff", "en.json", "tr.json");

    config.AddCommand<CheckCommand>("check")
        .WithDescription("Validate localization files against rules.")
        .WithExample("check", "./locales", "--rules", "no-empty-values,no-duplicate-keys");

    config.AddCommand<ConvertCommand>("convert")
        .WithDescription("Convert localization files between formats.")
        .WithExample("convert", "en.json", "en.yaml");

    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Generate skeleton target files from a base language.")
        .WithExample("generate", "tr", "--from", "en", "--in", "./locales", "--out", "./locales");

    config.AddCommand<WatchCommand>("watch")
        .WithDescription("Watch localization files for changes and re-run scan or check.")
        .WithExample("watch", "./locales", "--base", "en", "--mode", "scan");

    config.AddCommand<TranslateCommand>("translate")
        .WithDescription("Automatically translate localization files using external APIs.")
        .WithExample("translate", "tr", "--from", "en", "--in", "./locales", "--provider", "google");
});

return app.Run(args);