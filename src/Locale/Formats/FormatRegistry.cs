namespace Locale.Formats;

/// <summary>
/// Registry for localization format handlers.
/// </summary>
public sealed class FormatRegistry
{
    private readonly Dictionary<string, ILocalizationFormat> _formats = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ILocalizationFormat> _formatList = [];

    /// <summary>
    /// Gets the singleton default instance with all built-in formats registered.
    /// </summary>
    public static FormatRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Gets all registered formats.
    /// </summary>
    public IReadOnlyList<ILocalizationFormat> Formats => _formatList;

    /// <summary>
    /// Registers a format handler.
    /// </summary>
    public void Register(ILocalizationFormat format)
    {
        _formats[format.FormatId] = format;
        _formatList.Add(format);
    }

    /// <summary>
    /// Gets a format handler by its ID.
    /// </summary>
    public ILocalizationFormat? GetFormat(string formatId)
    {
        return _formats.TryGetValue(formatId, out ILocalizationFormat? format) ? format : null;
    }

    /// <summary>
    /// Gets a format handler for a file path based on its extension.
    /// </summary>
    public ILocalizationFormat? GetFormatForFile(string filePath)
    {
        return _formatList.FirstOrDefault(f => f.CanHandle(filePath));
    }

    /// <summary>
    /// Determines whether a file is supported by any registered format.
    /// </summary>
    public bool IsSupported(string filePath)
    {
        return _formatList.Any(f => f.CanHandle(filePath));
    }

    /// <summary>
    /// Gets all supported file extensions.
    /// </summary>
    public IEnumerable<string> GetSupportedExtensions()
    {
        return _formatList.SelectMany(f => f.SupportedExtensions).Distinct();
    }

    private static FormatRegistry CreateDefault()
    {
        FormatRegistry registry = new();
        registry.Register(new I18nextJsonLocalizationFormat());
        registry.Register(new VbResourceLocalizationFormat());
        registry.Register(new FluentFtlLocalizationFormat());
        registry.Register(new XliffLocalizationFormat());
        registry.Register(new YamlLocalizationFormat());
        registry.Register(new ResxLocalizationFormat());
        registry.Register(new JsonLocalizationFormat());
        registry.Register(new VttLocalizationFormat());
        registry.Register(new SrtLocalizationFormat());
        registry.Register(new CsvLocalizationFormat());
        registry.Register(new PoLocalizationFormat());
        return registry;
    }
}