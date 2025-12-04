using Locale.Models;
using System.Xml;

namespace Locale.Formats;

/// <summary>
/// Handler for XLIFF (XML Localization Interchange File Format) files.
/// </summary>
public sealed class XliffLocalizationFormat : LocalizationFormatBase
{
    /// <inheritdoc />
    public override string FormatId => "xliff";

    /// <inheritdoc />
    public override IReadOnlyList<string> SupportedExtensions => [".xlf", ".xliff"];

    /// <inheritdoc />
    public override LocalizationFile Parse(Stream stream, string? filePath = null)
    {
        List<LocalizationEntry> entries = [];

        XmlDocument doc = new();
        doc.Load(stream);

        XmlNamespaceManager nsManager = new(doc.NameTable);
        nsManager.AddNamespace("xlf", "urn:oasis:names:tc:xliff:document:1.2");
        nsManager.AddNamespace("xlf2", "urn:oasis:names:tc:xliff:document:2.0");

        // Try XLIFF 1.2
        XmlNodeList? transUnits = doc.SelectNodes("//xlf:trans-unit", nsManager);
        if (transUnits != null && transUnits.Count > 0)
        {
            foreach (XmlNode unit in transUnits)
            {
                string? id = unit.Attributes?["id"]?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                XmlNode? sourceNode = unit.SelectSingleNode("xlf:source", nsManager);
                XmlNode? targetNode = unit.SelectSingleNode("xlf:target", nsManager);
                XmlNode? noteNode = unit.SelectSingleNode("xlf:note", nsManager);

                entries.Add(new LocalizationEntry
                {
                    Key = id,
                    Source = sourceNode?.InnerText,
                    Value = targetNode?.InnerText ?? sourceNode?.InnerText,
                    Comment = noteNode?.InnerText
                });
            }
        }
        else
        {
            // Try XLIFF 2.0
            transUnits = doc.SelectNodes("//xlf2:unit", nsManager);
            if (transUnits != null)
            {
                foreach (XmlNode unit in transUnits)
                {
                    string? id = unit.Attributes?["id"]?.Value;
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    XmlNode? segmentNode = unit.SelectSingleNode("xlf2:segment", nsManager);
                    XmlNode? sourceNode = segmentNode?.SelectSingleNode("xlf2:source", nsManager);
                    XmlNode? targetNode = segmentNode?.SelectSingleNode("xlf2:target", nsManager);
                    XmlNode? notesNode = unit.SelectSingleNode("xlf2:notes/xlf2:note", nsManager);

                    entries.Add(new LocalizationEntry
                    {
                        Key = id,
                        Source = sourceNode?.InnerText,
                        Value = targetNode?.InnerText ?? sourceNode?.InnerText,
                        Comment = notesNode?.InnerText
                    });
                }
            }
        }

        // Fallback: try without namespace
        if (entries.Count == 0)
        {
            transUnits = doc.SelectNodes("//trans-unit");
            if (transUnits != null)
            {
                foreach (XmlNode unit in transUnits)
                {
                    string? id = unit.Attributes?["id"]?.Value;
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    XmlNode? sourceNode = unit.SelectSingleNode("source");
                    XmlNode? targetNode = unit.SelectSingleNode("target");
                    XmlNode? noteNode = unit.SelectSingleNode("note");

                    entries.Add(new LocalizationEntry
                    {
                        Key = id,
                        Source = sourceNode?.InnerText,
                        Value = targetNode?.InnerText ?? sourceNode?.InnerText,
                        Comment = noteNode?.InnerText
                    });
                }
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
        writer.WriteStartElement("xliff", "urn:oasis:names:tc:xliff:document:1.2");
        writer.WriteAttributeString("version", "1.2");

        writer.WriteStartElement("file");
        writer.WriteAttributeString("source-language", "en");
        writer.WriteAttributeString("target-language", file.Culture ?? "en");
        writer.WriteAttributeString("datatype", "plaintext");
        writer.WriteAttributeString("original", file.FilePath);

        writer.WriteStartElement("body");

        foreach (LocalizationEntry entry in file.Entries)
        {
            writer.WriteStartElement("trans-unit");
            writer.WriteAttributeString("id", entry.Key);

            writer.WriteStartElement("source");
            writer.WriteString(entry.Source ?? entry.Value ?? "");
            writer.WriteEndElement();

            writer.WriteStartElement("target");
            writer.WriteString(entry.Value ?? "");
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(entry.Comment))
            {
                writer.WriteStartElement("note");
                writer.WriteString(entry.Comment);
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // trans-unit
        }

        writer.WriteEndElement(); // body
        writer.WriteEndElement(); // file
        writer.WriteEndElement(); // xliff
        writer.WriteEndDocument();
    }
}