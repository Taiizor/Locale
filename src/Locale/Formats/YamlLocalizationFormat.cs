using Locale.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Locale.Formats;

/// <summary>
/// Handler for YAML localization files (flat and nested structures).
/// </summary>
public sealed class YamlLocalizationFormat : LocalizationFormatBase
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlLocalizationFormat"/> class
    /// with default YAML serialization settings.
    /// </summary>
    public YamlLocalizationFormat()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

        _serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();
    }

    /// <inheritdoc />
    public override string FormatId => "yaml";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".yaml", ".yml"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();

        List<LocalizationEntry> entries = [];
        object yamlObject = _deserializer.Deserialize<object>(content);

        if (yamlObject != null)
        {
            FlattenYamlObject(yamlObject, "", entries);
        }

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
        Dictionary<string, object> nested = BuildNestedStructure(file.Entries);
        string yaml = _serializer.Serialize(nested);

        using StreamWriter writer = new(stream, leaveOpen: true);
        writer.Write(yaml);
    }

    /// <summary>
    /// Recursively flattens a YAML object into key-value entries.
    /// </summary>
    private static void FlattenYamlObject(object obj, string prefix, List<LocalizationEntry> entries)
    {
        switch (obj)
        {
            case Dictionary<object, object> dict:
                foreach (KeyValuePair<object, object> kvp in dict)
                {
                    string key = string.IsNullOrEmpty(prefix)
                        ? kvp.Key.ToString() ?? ""
                        : $"{prefix}.{kvp.Key}";

                    if (kvp.Value == null)
                    {
                        entries.Add(new LocalizationEntry { Key = key, Value = null });
                    }
                    else
                    {
                        FlattenYamlObject(kvp.Value, key, entries);
                    }
                }
                break;

            case List<object> list:
                for (int i = 0; i < list.Count; i++)
                {
                    string key = $"{prefix}[{i}]";
                    if (list[i] == null)
                    {
                        entries.Add(new LocalizationEntry { Key = key, Value = null });
                    }
                    else
                    {
                        FlattenYamlObject(list[i], key, entries);
                    }
                }
                break;

            default:
                entries.Add(new LocalizationEntry
                {
                    Key = prefix,
                    Value = obj?.ToString()
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
}