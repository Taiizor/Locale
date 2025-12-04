using Locale.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace Locale.Formats;

/// <summary>
/// Handler for SRT subtitle files.
/// </summary>
public sealed partial class SrtLocalizationFormat : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "srt";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".srt"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        List<LocalizationEntry> entries = [];

        // Split by double newlines to get cues
        Regex cuePattern = CueRegex();
        MatchCollection matches = cuePattern.Matches(content);

        foreach (Match match in matches)
        {
            string index = match.Groups[1].Value;
            string timing = match.Groups[2].Value;
            string text = match.Groups[3].Value.Trim();

            // Use index as key
            string key = index;

            entries.Add(new LocalizationEntry
            {
                Key = key,
                Value = text,
                Comment = timing // Store timing as comment for preservation
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

        int index = 1;
        foreach (LocalizationEntry entry in file.Entries)
        {
            // Write cue number
            writer.WriteLine(index);

            // Write timing (use stored comment or generate default)
            string timing = entry.Comment ?? $"00:00:{index:D2},000 --> 00:00:{index + 1:D2},000";
            writer.WriteLine(timing);

            // Write text
            writer.WriteLine(entry.Value ?? "");
            writer.WriteLine();

            index++;
        }
    }

    [GeneratedRegex(@"(\d+)\r?\n(\d{2}:\d{2}:\d{2},\d{3}\s*-->\s*\d{2}:\d{2}:\d{2},\d{3})\r?\n([\s\S]*?)(?=\r?\n\r?\n|\r?\n$|$)", RegexOptions.Multiline)]
    private static partial Regex CueRegex();
}