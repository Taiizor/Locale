namespace Locale.Services;

/// <summary>
/// Helper class for path manipulation in localization services.
/// </summary>
internal static class PathHelper
{
    /// <summary>
    /// Known multi-part extensions that should be treated as a single extension.
    /// </summary>
    private static readonly string[] MultiPartExtensions =
    [
        ".i18n.json"
    ];

    /// <summary>
    /// Gets the extension of a file, including multi-part extensions like ".i18n.json".
    /// </summary>
    /// <param name="filePath">The file path to get the extension from.</param>
    /// <returns>The extension including the leading dot.</returns>
    public static string GetExtension(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        foreach (string multiPartExt in MultiPartExtensions)
        {
            if (fileName.EndsWith(multiPartExt, StringComparison.OrdinalIgnoreCase))
            {
                return multiPartExt;
            }
        }

        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// Gets the file name without extension, handling multi-part extensions like ".i18n.json".
    /// </summary>
    /// <param name="filePath">The file path to get the name from.</param>
    /// <returns>The file name without extension.</returns>
    public static string GetFileNameWithoutExtension(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        foreach (string multiPartExt in MultiPartExtensions)
        {
            if (fileName.EndsWith(multiPartExt, StringComparison.OrdinalIgnoreCase))
            {
                return fileName[..^multiPartExt.Length];
            }
        }

        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Generates a target file path by replacing the source culture with the target culture.
    /// </summary>
    /// <param name="sourceFilePath">The source file path.</param>
    /// <param name="inputPath">The input path (file or directory).</param>
    /// <param name="outputPath">The output path.</param>
    /// <param name="sourceCulture">The source culture code.</param>
    /// <param name="targetCulture">The target culture code.</param>
    /// <returns>The generated target file path.</returns>
    public static string GenerateTargetPath(string sourceFilePath, string inputPath, string outputPath,
        string sourceCulture, string targetCulture)
    {
        string fileName = Path.GetFileName(sourceFilePath);
        string extension = GetExtension(sourceFilePath);
        string nameWithoutExt = GetFileNameWithoutExtension(sourceFilePath);

        string targetFileName;
        if (nameWithoutExt.EndsWith($".{sourceCulture}", StringComparison.OrdinalIgnoreCase))
        {
            targetFileName = nameWithoutExt[..^(sourceCulture.Length + 1)] + $".{targetCulture}{extension}";
        }
        else if (nameWithoutExt.Equals(sourceCulture, StringComparison.OrdinalIgnoreCase))
        {
            targetFileName = $"{targetCulture}{extension}";
        }
        else
        {
            targetFileName = $"{nameWithoutExt}.{targetCulture}{extension}";
        }

        string relativePath;
        if (File.Exists(inputPath))
        {
            relativePath = targetFileName;
        }
        else
        {
            string relativeDir = Path.GetRelativePath(inputPath, Path.GetDirectoryName(sourceFilePath) ?? "");
            relativePath = Path.Combine(relativeDir, targetFileName);
        }

        string targetPath = Path.Combine(outputPath, relativePath);

        string? targetDir = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        return targetPath;
    }
}