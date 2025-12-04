using Locale.Models;
using System.Xml;

namespace Locale.Formats;

/// <summary>
/// Handler for .NET RESX (XML resource) localization files.
/// </summary>
public sealed class ResxLocalizationFormat : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "resx";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".resx"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        List<LocalizationEntry> entries = [];

        XmlDocument doc = new();
        doc.Load(stream);

        XmlNodeList? dataNodes = doc.SelectNodes("//data");
        if (dataNodes != null)
        {
            foreach (XmlNode dataNode in dataNodes)
            {
                string? name = dataNode.Attributes?["name"]?.Value;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                XmlNode? valueNode = dataNode.SelectSingleNode("value");
                string? value = valueNode?.InnerText;

                XmlNode? commentNode = dataNode.SelectSingleNode("comment");
                string? comment = commentNode?.InnerText;

                // Optionally convert underscore-separated keys to dot notation
                string key = ConvertKeyStyle(name);

                entries.Add(new LocalizationEntry
                {
                    Key = key,
                    Value = value,
                    Comment = comment
                });
            }
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
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = Environment.NewLine
        };

        using XmlWriter writer = XmlWriter.Create(stream, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("root");

        // Write standard RESX headers
        WriteResxHeaders(writer);

        // Write data entries
        foreach (LocalizationEntry entry in file.Entries)
        {
            writer.WriteStartElement("data");
            writer.WriteAttributeString("name", ConvertKeyToUnderscore(entry.Key));
            writer.WriteAttributeString("xml", "space", null, "preserve");

            writer.WriteStartElement("value");
            writer.WriteString(entry.Value ?? "");
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(entry.Comment))
            {
                writer.WriteStartElement("comment");
                writer.WriteString(entry.Comment);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement(); // root
        writer.WriteEndDocument();
    }

    private static void WriteResxHeaders(XmlWriter writer)
    {
        // Write schema and header elements typical for RESX files
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "resmimetype");
        writer.WriteElementString("value", "text/microsoft-resx");
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "version");
        writer.WriteElementString("value", "2.0");
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "reader");
        writer.WriteElementString("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "writer");
        writer.WriteElementString("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();
    }

    /// <summary>
    /// Converts underscore-separated keys to dot notation (e.g., "Home_Title" -> "Home.Title").
    /// </summary>
    private static string ConvertKeyStyle(string key)
    {
        return key.Replace("_", ".");
    }

    /// <summary>
    /// Converts dot notation keys to underscore-separated (e.g., "Home.Title" -> "Home_Title").
    /// </summary>
    private static string ConvertKeyToUnderscore(string key)
    {
        return key.Replace(".", "_");
    }
}