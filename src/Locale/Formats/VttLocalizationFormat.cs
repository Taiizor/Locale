using Locale.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Locale.Formats;

/// <summary>
/// Handler for WebVTT subtitle files.
/// </summary>
public sealed partial class VttLocalizationFormat : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "vtt";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".vtt"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        List<LocalizationEntry> entries = [];

        // Skip WEBVTT header
        string[] lines = content.Split('\n');
        bool inCue = false;
        string cueId = "";
        string timing = "";
        List<string> textLines = [];
        int index = 1;

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd('\r');

            // Skip header
            if (line.StartsWith("WEBVTT"))
            {
                continue;
            }

            // Empty line ends a cue
            if (string.IsNullOrWhiteSpace(line))
            {
                if (inCue && textLines.Count > 0)
                {
                    string key = string.IsNullOrEmpty(cueId) ? index.ToString() : cueId;
                    entries.Add(new LocalizationEntry
                    {
                        Key = key,
                        Value = string.Join("\n", textLines),
                        Comment = timing
                    });
                    index++;
                }

                inCue = false;
                cueId = "";
                timing = "";
                textLines.Clear();
                continue;
            }

            // Check for timing line
            if (TimingRegex().IsMatch(line))
            {
                timing = line;
                inCue = true;
                continue;
            }

            // Check for cue identifier (line before timing that's not timing)
            if (!inCue && !string.IsNullOrWhiteSpace(line) && !TimingRegex().IsMatch(line))
            {
                cueId = line;
                continue;
            }

            // Collect text lines
            if (inCue)
            {
                textLines.Add(line);
            }
        }

        // Handle last cue
        if (inCue && textLines.Count > 0)
        {
            string key = string.IsNullOrEmpty(cueId) ? index.ToString() : cueId;
            entries.Add(new LocalizationEntry
            {
                Key = key,
                Value = string.Join("\n", textLines),
                Comment = timing
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

        writer.WriteLine("WEBVTT");
        writer.WriteLine();

        int index = 1;
        foreach (LocalizationEntry entry in file.Entries)
        {
            // Write cue identifier if not numeric
            if (!int.TryParse(entry.Key, out _))
            {
                writer.WriteLine(entry.Key);
            }

            // Write timing (use stored comment or generate default)
            string timing = entry.Comment ?? $"00:00:{index:D2}.000 --> 00:00:{index + 1:D2}.000";
            writer.WriteLine(timing);

            // Write text
            writer.WriteLine(entry.Value ?? "");
            writer.WriteLine();

            index++;
        }
    }

    [GeneratedRegex(@"^\d{2}:\d{2}:\d{2}[.,]\d{3}\s*-->\s*\d{2}:\d{2}:\d{2}[.,]\d{3}")]
    private static partial Regex TimingRegex();
}