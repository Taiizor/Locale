using Locale.Models;
using System.Text.Json;

namespace Locale.Formats;

/// <summary>
/// Handler for JSON localization files (flat and nested structures).
/// </summary>
public sealed class JsonLocalizationFormat : LocalizationFormatBase
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonWriterOptions WriteOptions = new()
    {
        Indented = true
    };

    /// <inheritdoc />
    public override string FormatId => "json";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".json"];

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
            Culture = DetectCultureFromFileName(filePath),
            Format = FormatId,
            Entries = entries
        };
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
                // Arrays are typically not used for translations but we handle them
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string key = $"{prefix}[{index}]";
                    FlattenJsonElement(item, key, entries);
                    index++;
                }
                break;

            default:
                // For numbers, booleans, etc., convert to string
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
                // Conflict: existing value is not a dictionary
                // Create a new nested structure
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