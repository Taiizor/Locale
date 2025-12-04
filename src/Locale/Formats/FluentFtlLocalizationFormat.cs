using Locale.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Locale.Formats;

/// <summary>
/// Handler for Mozilla Fluent FTL localization files.
/// Provides read-oriented support with best-effort write behavior.
/// </summary>
public sealed partial class FluentFtlLocalizationFormat : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "ftl";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".ftl"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        List<LocalizationEntry> entries = [];

        // Parse Fluent messages
        string[] lines = content.Split('\n');
        string? currentKey = null;
        StringBuilder currentValue = new();
        string? currentComment = null;
        bool inMultiline = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');

            // Handle comments
            if (line.StartsWith('#'))
            {
                if (line.StartsWith("# "))
                {
                    currentComment = line[2..];
                }
                continue;
            }

            // Empty line ends current message
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentKey != null)
                {
                    entries.Add(new LocalizationEntry
                    {
                        Key = currentKey,
                        Value = currentValue.ToString().Trim(),
                        Comment = currentComment
                    });
                    currentKey = null;
                    currentValue.Clear();
                    currentComment = null;
                    inMultiline = false;
                }
                continue;
            }

            // Check for new message definition
            Match messageMatch = MessageRegex().Match(line);
            if (messageMatch.Success)
            {
                // Save previous message
                if (currentKey != null)
                {
                    entries.Add(new LocalizationEntry
                    {
                        Key = currentKey,
                        Value = currentValue.ToString().Trim(),
                        Comment = currentComment
                    });
                    currentValue.Clear();
                    currentComment = null;
                }

                currentKey = messageMatch.Groups[1].Value;
                string value = messageMatch.Groups[2].Value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    inMultiline = true;
                }
                else
                {
                    currentValue.Append(value);
                    inMultiline = false;
                }
                continue;
            }

            // Handle attributes (key.attribute = value)
            Match attrMatch = AttributeRegex().Match(line);
            if (attrMatch.Success && currentKey != null)
            {
                // Save current message first
                if (currentValue.Length > 0)
                {
                    entries.Add(new LocalizationEntry
                    {
                        Key = currentKey,
                        Value = currentValue.ToString().Trim(),
                        Comment = currentComment
                    });
                    currentValue.Clear();
                    currentComment = null;
                }

                // Add attribute as separate key
                string attrKey = $"{currentKey}.{attrMatch.Groups[1].Value}";
                string attrValue = attrMatch.Groups[2].Value.Trim();
                entries.Add(new LocalizationEntry
                {
                    Key = attrKey,
                    Value = attrValue
                });
                currentKey = null;
                continue;
            }

            // Handle continuation lines (multiline values)
            if (inMultiline && line.StartsWith("    "))
            {
                if (currentValue.Length > 0)
                {
                    currentValue.Append('\n');
                }

                currentValue.Append(line.TrimStart());
            }
        }

        // Add last message
        if (currentKey != null)
        {
            entries.Add(new LocalizationEntry
            {
                Key = currentKey,
                Value = currentValue.ToString().Trim(),
                Comment = currentComment
            });
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
        using StreamWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);

        writer.WriteLine("# Fluent FTL file generated by Locale");
        writer.WriteLine();

        foreach (LocalizationEntry entry in file.Entries)
        {
            // Skip attribute entries (they'll be written with their parent)
            if (entry.Key.Contains('.'))
            {
                string[] parts = entry.Key.Split('.', 2);
                string parentKey = parts[0];
                string attrName = parts[1];

                // Check if this is truly an attribute (parent exists without this suffix)
                bool isAttribute = file.Entries.Any(e => e.Key == parentKey);
                if (isAttribute)
                {
                    continue; // Will be handled when writing parent
                }
            }

            if (!string.IsNullOrEmpty(entry.Comment))
            {
                writer.WriteLine($"# {entry.Comment}");
            }

            string value = entry.Value ?? "";

            // Check if value needs multiline formatting
            if (value.Contains('\n'))
            {
                writer.WriteLine($"{entry.Key} =");
                foreach (string line in value.Split('\n'))
                {
                    writer.WriteLine($"    {line}");
                }
            }
            else
            {
                writer.WriteLine($"{entry.Key} = {value}");
            }

            // Write any attributes for this key
            foreach (LocalizationEntry? attr in file.Entries.Where(e => e.Key.StartsWith($"{entry.Key}.") && e.Key != entry.Key))
            {
                string attrName = attr.Key[(entry.Key.Length + 1)..];
                writer.WriteLine($"    .{attrName} = {attr.Value}");
            }

            writer.WriteLine();
        }
    }

    [GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9_-]*)\s*=\s*(.*)$")]
    private static partial Regex MessageRegex();

    [GeneratedRegex(@"^\s*\.([a-zA-Z][a-zA-Z0-9_-]*)\s*=\s*(.*)$")]
    private static partial Regex AttributeRegex();
}