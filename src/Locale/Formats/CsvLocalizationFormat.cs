using Locale.Models;
using System.Text;

namespace Locale.Formats;

/// <summary>
/// Handler for CSV localization files.
/// Supports both 2-column (key,value) and multi-language (key,en,tr,de) formats.
/// </summary>
public sealed class CsvLocalizationFormat(char delimiter = ',') : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "csv";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".csv"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using StreamReader reader = new(stream);
        List<LocalizationEntry> entries = [];

        string? headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
        {
            return new LocalizationFile
            {
                FilePath = filePath ?? "",
                Culture = DetectCultureFromFileName(filePath),
                Format = FormatId,
                Entries = entries
            };
        }

        List<string> headers = ParseCsvLine(headerLine);
        string? culture = DetectCultureFromFileName(filePath);

        // Determine column index for value
        int valueColumnIndex = 1; // Default: second column

        // If culture is detected, try to find matching column
        if (!string.IsNullOrEmpty(culture) && headers.Count > 2)
        {
            for (int i = 1; i < headers.Count; i++)
            {
                if (headers[i].Equals(culture, StringComparison.OrdinalIgnoreCase))
                {
                    valueColumnIndex = i;
                    break;
                }
            }
        }

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            List<string> values = ParseCsvLine(line);
            if (values.Count < 2)
            {
                continue;
            }

            string key = values[0];
            string value = valueColumnIndex < values.Count ? values[valueColumnIndex] : "";

            entries.Add(new LocalizationEntry
            {
                Key = key,
                Value = value
            });
        }

        return new LocalizationFile
        {
            FilePath = filePath ?? "",
            Culture = culture,
            Format = FormatId,
            Entries = entries
        };
    }

    /// <inheritdoc />
    public override void Write(LocalizationFile file, Stream stream)
    {
        using StreamWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);

        // Write header
        writer.WriteLine($"key{delimiter}value");

        // Write entries
        foreach (LocalizationEntry entry in file.Entries)
        {
            writer.WriteLine($"{EscapeCsv(entry.Key)}{delimiter}{EscapeCsv(entry.Value ?? "")}");
        }
    }

    private List<string> ParseCsvLine(string line)
    {
        List<string> values = [];
        StringBuilder current = new();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private string EscapeCsv(string value)
    {
        if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}