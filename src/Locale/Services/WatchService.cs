using Locale.Formats;
using Locale.Models;

namespace Locale.Services;

/// <summary>
/// Options for the watch operation.
/// </summary>
public sealed class WatchOptions
{
    /// <summary>
    /// Gets or sets the base culture to compare against.
    /// </summary>
    public string BaseCulture { get; set; } = "en";

    /// <summary>
    /// Gets or sets the target cultures to compare.
    /// </summary>
    public List<string> TargetCultures { get; set; } = [];

    /// <summary>
    /// Gets or sets the command to run on changes (scan or check).
    /// </summary>
    public WatchMode Mode { get; set; } = WatchMode.Scan;

    /// <summary>
    /// Gets or sets whether to include subdirectories.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the debounce interval in milliseconds.
    /// </summary>
    public int DebounceMs { get; set; } = 500;
}

/// <summary>
/// Watch mode (what to run on file changes).
/// </summary>
public enum WatchMode
{
    /// <summary>
    /// Run the scan command when files change.
    /// </summary>
    Scan,

    /// <summary>
    /// Run the check command when files change.
    /// </summary>
    Check
}

/// <summary>
/// Service for watching localization files for changes.
/// </summary>
public sealed class WatchService(FormatRegistry registry) : IDisposable
{
    private readonly ScanService _scanService = new(registry);
    private readonly CheckService _checkService = new(registry);
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchService"/> class with the default format registry.
    /// </summary>
    public WatchService() : this(FormatRegistry.Default)
    {
    }

    /// <summary>
    /// Event raised when a scan or check completes after file changes.
    /// </summary>
    public event EventHandler<WatchEventArgs>? OnChange;

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    public event EventHandler<WatchErrorEventArgs>? OnError;

    /// <summary>
    /// Starts watching the specified directory for changes.
    /// </summary>
    public void Start(string path, WatchOptions options)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        Stop();

        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = options.Recursive,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
        };

        // Add filters for all supported extensions
        foreach (string ext in registry.GetSupportedExtensions())
        {
            _watcher.Filters.Add($"*{ext}");
        }

        void HandleChange(object sender, FileSystemEventArgs e)
        {
            // Debounce file changes
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(_ =>
            {
                try
                {
                    RunCommand(path, options);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new WatchErrorEventArgs(ex.Message));
                }
            }, null, options.DebounceMs, Timeout.Infinite);
        }

        _watcher.Changed += HandleChange;
        _watcher.Created += HandleChange;
        _watcher.Deleted += HandleChange;
        _watcher.Renamed += HandleChange;
    }

    /// <summary>
    /// Stops watching for changes.
    /// </summary>
    public void Stop()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = null;

        _watcher?.Dispose();
        _watcher = null;
    }

    private void RunCommand(string path, WatchOptions options)
    {
        if (options.Mode == WatchMode.Scan)
        {
            ScanOptions scanOptions = new()
            {
                BaseCulture = options.BaseCulture,
                TargetCultures = options.TargetCultures,
                Recursive = options.Recursive
            };

            ScanReport report = _scanService.Scan(path, scanOptions);
            OnChange?.Invoke(this, new WatchEventArgs(report));
        }
        else
        {
            CheckOptions checkOptions = new()
            {
                Recursive = options.Recursive
            };

            CheckReport report = _checkService.Check(path, checkOptions);
            OnChange?.Invoke(this, new WatchEventArgs(report));
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="WatchService"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Stop();
    }
}

/// <summary>
/// Event arguments for watch change events.
/// </summary>
public class WatchEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WatchEventArgs"/> class with a scan report.
    /// </summary>
    /// <param name="scanReport">The scan report from the triggered scan operation.</param>
    public WatchEventArgs(ScanReport scanReport)
    {
        ScanReport = scanReport;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchEventArgs"/> class with a check report.
    /// </summary>
    /// <param name="checkReport">The check report from the triggered check operation.</param>
    public WatchEventArgs(CheckReport checkReport)
    {
        CheckReport = checkReport;
    }

    /// <summary>
    /// Gets the scan report, if the watch mode was set to scan.
    /// </summary>
    public ScanReport? ScanReport { get; }

    /// <summary>
    /// Gets the check report, if the watch mode was set to check.
    /// </summary>
    public CheckReport? CheckReport { get; }
}

/// <summary>
/// Event arguments for watch error events.
/// </summary>
/// <param name="message">The error message describing what went wrong.</param>
public class WatchErrorEventArgs(string message) : EventArgs
{
    /// <summary>
    /// Gets the error message describing what went wrong.
    /// </summary>
    public string Message { get; } = message;
}