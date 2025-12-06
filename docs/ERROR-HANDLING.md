# Error Handling Guidelines

This document outlines the error handling patterns and best practices used in the Locale project.

## Exception Types

### 1. `ArgumentException` / `ArgumentNullException`
Use for invalid method arguments:

```csharp
public void Convert(string inputPath, string outputPath, ConvertOptions options)
{
    if (string.IsNullOrWhiteSpace(inputPath))
        throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));
    
    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
    
    if (options == null)
        throw new ArgumentNullException(nameof(options));
}
```

### 2. `InvalidOperationException`
Use for malformed files or invalid state:

```csharp
public LocalizationFile Parse(Stream stream, string? filePath = null)
{
    var doc = XDocument.Load(stream);
    
    if (doc.Root?.Name != "resources")
        throw new InvalidOperationException(
            $"Invalid format: Expected 'resources' root element, found '{doc.Root?.Name}'"
        );
    
    // ... parsing logic
}
```

### 3. `FileNotFoundException` / `DirectoryNotFoundException`
Use for missing file system resources:

```csharp
public ScanReport Scan(string path, ScanOptions options)
{
    if (!Directory.Exists(path) && !File.Exists(path))
        throw new FileNotFoundException(
            $"Path not found: {path}. Please verify the path exists."
        );
    
    // ... scan logic
}
```

### 4. `NotSupportedException`
Use for unsupported operations:

```csharp
public void Write(LocalizationFile file, Stream stream)
{
    if (file.Format == "vb")
        throw new NotSupportedException(
            "VB resource format is read-only. Please convert to RESX or another writable format."
        );
    
    // ... write logic
}
```

## User-Friendly Error Messages

### Guidelines for Error Messages

1. **Be specific** - Tell the user exactly what went wrong
2. **Be actionable** - Suggest how to fix the problem
3. **Provide context** - Include relevant details (file path, line number, etc.)
4. **Use plain language** - Avoid technical jargon when possible

### Examples

#### ❌ Bad Error Messages
```csharp
throw new Exception("Invalid file");
throw new Exception("Error");
throw new Exception("Parsing failed");
```

#### ✅ Good Error Messages
```csharp
throw new InvalidOperationException(
    $"Failed to parse '{filePath}': Invalid JSON format at line 15. " +
    "Expected closing brace '}}' for object."
);

throw new ArgumentException(
    $"Culture code '{culture}' is not valid. " +
    "Please use standard culture codes like 'en', 'tr', 'de-DE'. " +
    "See https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/",
    nameof(culture)
);

throw new FileNotFoundException(
    $"Translation file not found: {filePath}\n" +
    "Did you mean: {Path.Combine(directory, "en.json")}?\n" +
    "Available files: {string.Join(", ", availableFiles)}"
);
```

## CLI Error Handling

### Exit Codes

The CLI should use standard exit codes:

```csharp
public enum ExitCode
{
    Success = 0,           // Operation completed successfully
    ValidationError = 1,   // Validation rules failed (--ci mode)
    UserError = 2,         // User error (invalid arguments, missing files)
    UnexpectedError = 3    // Unexpected internal error
}
```

### Error Display

Use Spectre.Console for user-friendly error display:

```csharp
try
{
    // Operation
}
catch (ArgumentException ex)
{
    AnsiConsole.MarkupLine("[red]❌ Invalid argument:[/]");
    AnsiConsole.MarkupLine($"  {ex.Message}");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[yellow]💡 Tip:[/] Check your command syntax with --help");
    return (int)ExitCode.UserError;
}
catch (FileNotFoundException ex)
{
    AnsiConsole.MarkupLine("[red]❌ File not found:[/]");
    AnsiConsole.MarkupLine($"  {ex.Message}");
    return (int)ExitCode.UserError;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine("[red]❌ Unexpected error:[/]");
    AnsiConsole.WriteException(ex);
    return (int)ExitCode.UnexpectedError;
}
```

### User-Friendly CLI Errors

#### Example: Missing File
```
❌ File not found:
  ./locales/en.json

💡 Did you mean?
  • ./locales/en-US.json
  • ./locale/en.json (different directory)

💡 Tip: Use 'locale scan ./locales' to see available files
```

#### Example: Validation Failed
```
❌ Translation validation failed:

en → tr:
  • 5 missing keys
  • 2 empty values

en → de:
  • 12 missing keys
  • 1 placeholder mismatch

💡 Tip: Run 'locale scan ./locales --base en --targets tr,de' for details
```

## Library Error Handling

### Validation Methods

Create helper methods for common validations:

```csharp
internal static class Validate
{
    public static void NotNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                $"{paramName} cannot be null or empty", 
                paramName
            );
    }
    
    public static void PathExists(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            throw new FileNotFoundException(
                $"Path not found: {path}"
            );
    }
    
    public static void CultureCode(string? culture, string paramName)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return; // Optional culture
        
        try
        {
            CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            throw new ArgumentException(
                $"Invalid culture code: '{culture}'. " +
                "Use standard codes like 'en', 'tr', 'de-DE'.",
                paramName
            );
        }
    }
}
```

Usage:
```csharp
public ScanReport Scan(string path, ScanOptions options)
{
    Validate.NotNullOrEmpty(path, nameof(path));
    Validate.PathExists(path);
    Validate.CultureCode(options.BaseCulture, nameof(options.BaseCulture));
    
    // ... scan logic
}
```

## Async Error Handling

### For Translation Services

```csharp
public async Task<List<TranslationResult>> TranslateAsync(
    string path, 
    TranslateOptions options,
    CancellationToken cancellationToken = default)
{
    try
    {
        // API call
        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await ParseResponse(response);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
    {
        throw new InvalidOperationException(
            $"Authentication failed for {options.Provider}. " +
            "Please check your API key is correct and has sufficient permissions.",
            ex
        );
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
    {
        throw new InvalidOperationException(
            "Rate limit exceeded. " +
            "Try using --delay to add delay between requests, " +
            "or reduce --parallel to make fewer concurrent requests.",
            ex
        );
    }
    catch (TaskCanceledException ex)
    {
        throw new OperationCanceledException(
            "Translation was cancelled. " +
            "This may happen if the API is slow or timeout is too short.",
            ex
        );
    }
}
```

## Error Recovery and Fallbacks

### Partial Success Pattern

For operations that can partially succeed:

```csharp
public class ConversionResult
{
    public List<string> Succeeded { get; } = [];
    public List<ConversionError> Failed { get; } = [];
    
    public bool HasErrors => Failed.Count > 0;
    public bool IsPartialSuccess => Succeeded.Count > 0 && Failed.Count > 0;
}

public class ConversionError
{
    public required string FilePath { get; init; }
    public required string Message { get; init; }
    public Exception? Exception { get; init; }
}
```

Usage:
```csharp
public ConversionResult ConvertDirectory(string sourceDir, string targetDir, ConvertOptions options)
{
    var result = new ConversionResult();
    
    foreach (var file in Directory.GetFiles(sourceDir))
    {
        try
        {
            Convert(file, GetTargetPath(file, targetDir), options);
            result.Succeeded.Add(file);
        }
        catch (Exception ex)
        {
            result.Failed.Add(new ConversionError
            {
                FilePath = file,
                Message = ex.Message,
                Exception = ex
            });
        }
    }
    
    return result;
}
```

## Testing Error Conditions

Always test error paths:

```csharp
[Fact]
public void Scan_NonExistentPath_ThrowsFileNotFoundException()
{
    var service = new ScanService();
    var options = new ScanOptions { BaseCulture = "en" };
    
    var exception = Assert.Throws<FileNotFoundException>(
        () => service.Scan("/nonexistent/path", options)
    );
    
    Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void Parse_InvalidJson_ThrowsInvalidOperationException()
{
    var format = new JsonLocalizationFormat();
    var invalidJson = "{ invalid json";
    
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
    
    var exception = Assert.Throws<InvalidOperationException>(
        () => format.Parse(stream, "test.json")
    );
    
    Assert.Contains("Invalid JSON", exception.Message);
}
```

## Documentation

Always document exceptions in XML comments:

```csharp
/// <summary>
/// Scans a directory for localization files and compares base to target cultures.
/// </summary>
/// <param name="path">The directory or file path to scan.</param>
/// <param name="options">Configuration options for the scan.</param>
/// <returns>A report containing missing keys, orphan keys, and empty values.</returns>
/// <exception cref="ArgumentException">
/// Thrown when <paramref name="path"/> is null or empty.
/// </exception>
/// <exception cref="FileNotFoundException">
/// Thrown when <paramref name="path"/> does not exist.
/// </exception>
/// <exception cref="InvalidOperationException">
/// Thrown when a file cannot be parsed due to invalid format.
/// </exception>
public ScanReport Scan(string path, ScanOptions options)
{
    // Implementation
}
```

## Summary

1. **Use appropriate exception types** for different scenarios
2. **Write user-friendly error messages** with context and suggestions
3. **Return appropriate exit codes** in CLI applications
4. **Use partial success patterns** for batch operations
5. **Document all exceptions** in XML comments
6. **Test error conditions** thoroughly
7. **Consider logging** for debugging production issues