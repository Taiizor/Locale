using Locale.Models;
using System.Text.Json;

namespace Locale.Formats;

/// <summary>
/// Handler for i18next-style JSON localization files.
/// Handles nested JSON structures commonly used by i18next with namespace support.
/// </summary>
public sealed class I18nextJsonLocalizationFormat : LocalizationFormatBase
{
    private static readonly JsonWriterOptions WriteOptions = new()
    {
        Indented = true
    };

    /// <inheritdoc />
    public override string FormatId => "i18next";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".i18n.json"];

    /// <inheritdoc />
    public override bool CanHandle(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        // Check for i18next-style naming patterns
        string fileName = Path.GetFileName(filePath).ToLowerInvariant();
        return fileName.EndsWith(".i18n.json") ||
               fileName.Contains("i18next") ||
               fileName.Contains("translation.json");
    }

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using JsonDocument document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        List<LocalizationEntry> entries = [];
        FlattenJsonElement(document.RootElement, "", entries);

        return new LocalizationFile
        {
            FilePath = filePath ?? "",
            Culture = DetectI18nextCulture(filePath),
            Format = FormatId,
            Entries = entries
        };
    }

    /// <summary>
    /// Detects culture from i18next-style file names like "en.i18n.json" or "locales/en/translation.json".
    /// </summary>
    private static string? DetectI18nextCulture(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Handle "en.i18n.json" -> remove ".i18n" to get "en"
        if (fileName.EndsWith(".i18n"))
        {
            string culture = fileName[..^5]; // Remove ".i18n"
            if (LooksLikeCultureCode(culture))
            {
                return culture.ToLowerInvariant();
            }
        }

        // Handle "locales/en/translation.json" -> parent directory is culture
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            string dirName = Path.GetFileName(directory);
            if (LooksLikeCultureCode(dirName))
            {
                return dirName.ToLowerInvariant();
            }
        }

        // Fallback to base class detection
        return DetectCultureFromFileName(filePath);
    }

    /// <summary>
    /// Checks if a string looks like a culture code.
    /// </summary>
    private static bool LooksLikeCultureCode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Simple culture codes: en, tr, de, etc.
        if (value.Length == 2 && value.All(char.IsLetter))
        {
            return true;
        }

        // Full culture codes: en-US, tr-TR, de-DE, etc.
        if (value.Length >= 4 && value.Length <= 5 && value.Contains('-'))
        {
            string[] cultureParts = value.Split('-');
            return cultureParts.Length == 2 &&
                   cultureParts[0].Length == 2 &&
                   cultureParts[0].All(char.IsLetter) &&
                   cultureParts[1].All(char.IsLetterOrDigit);
        }

        return false;
    }

    /// <inheritdoc />
    public override void Write(LocalizationFile file, Stream stream)
    {
        using Utf8JsonWriter writer = new(stream, WriteOptions);

        Dictionary<string, object> root = BuildNestedStructure(file.Entries);
        WriteJsonObject(writer, root);
    }

    /// <summary>
    /// Recursively flattens a JSON element into key-value entries.
    /// Handles i18next-specific structures like namespaces and pluralization.
    /// </summary>
    private static void FlattenJsonElement(JsonElement element, string prefix, List<LocalizationEntry> entries)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenJsonElement(property.Value, key, entries);
                }
                break;

            case JsonValueKind.String:
                entries.Add(new LocalizationEntry
                {
                    Key = prefix,
                    Value = element.GetString()
                });
                break;

            case JsonValueKind.Null:
                entries.Add(new LocalizationEntry
                {
                    Key = prefix,
                    Value = null
                });
                break;

            case JsonValueKind.Array:
                // i18next can use arrays for pluralization
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string key = $"{prefix}[{index}]";
                    FlattenJsonElement(item, key, entries);
                    index++;
                }
                break;

            default:
                entries.Add(new LocalizationEntry
                {
                    Key = prefix,
                    Value = element.GetRawText()
                });
                break;
        }
    }

    /// <summary>
    /// Builds a nested dictionary structure from flat entries.
    /// </summary>
    private static Dictionary<string, object> BuildNestedStructure(IEnumerable<LocalizationEntry> entries)
    {
        Dictionary<string, object> root = [];

        foreach (LocalizationEntry entry in entries)
        {
            SetNestedValue(root, entry.Key, entry.Value ?? "");
        }

        return root;
    }

    /// <summary>
    /// Sets a value in a nested dictionary structure using a dot-separated key.
    /// </summary>
    private static void SetNestedValue(Dictionary<string, object> dict, string key, string value)
    {
        string[] parts = key.Split('.');
        Dictionary<string, object> current = dict;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (!current.TryGetValue(parts[i], out object? existing))
            {
                Dictionary<string, object> nested = [];
                current[parts[i]] = nested;
                current = nested;
            }
            else if (existing is Dictionary<string, object> nestedDict)
            {
                current = nestedDict;
            }
            else
            {
                Dictionary<string, object> nested = [];
                current[parts[i]] = nested;
                current = nested;
            }
        }

        string finalKey = parts[^1];
        if (!string.IsNullOrEmpty(finalKey))
        {
            current[finalKey] = value;
        }
    }

    /// <summary>
    /// Writes a dictionary as a JSON object.
    /// </summary>
    private static void WriteJsonObject(Utf8JsonWriter writer, Dictionary<string, object> dict)
    {
        writer.WriteStartObject();

        foreach (KeyValuePair<string, object> kvp in dict)
        {
            writer.WritePropertyName(kvp.Key);

            if (kvp.Value is Dictionary<string, object> nested)
            {
                WriteJsonObject(writer, nested);
            }
            else
            {
                writer.WriteStringValue(kvp.Value?.ToString() ?? "");
            }
        }

        writer.WriteEndObject();
    }
}