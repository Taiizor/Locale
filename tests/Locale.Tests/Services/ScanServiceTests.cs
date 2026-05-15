using Locale.Models;
using Locale.Services;

namespace Locale.Tests.Services;

public class ScanServiceTests
{
    private readonly ScanService _service = new();

    [Fact]
    public void Scan_EmptyPath_ReturnsEmptyReport()
    {
        // Using a non-existent path should return an empty report
        ScanOptions options = new()
        {
            BaseCulture = "en",
            TargetCultures = ["tr"]
        };

        ScanReport report = _service.Scan("/non/existent/path", options);

        Assert.Equal("en", report.BaseCulture);
        Assert.False(report.HasIssues);
    }

    [Fact]
    public void Scan_WithTempDirectory_DetectsMissingKeys()
    {
        // Create temporary test files
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create base file (en.json)
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello",
                    "world": "World",
                    "goodbye": "Goodbye"
                }
                """);

            // Create target file (tr.json) with missing key
            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba",
                    "world": "Dünya"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.True(report.HasIssues);
            Assert.Single(report.Results);
            Assert.Contains("goodbye", report.Results[0].MissingKeys);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_WithTempDirectory_DetectsOrphanKeys()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create base file (en.json)
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello"
                }
                """);

            // Create target file (tr.json) with extra key
            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba",
                    "orphan_key": "Yetim anahtar"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.True(report.HasIssues);
            Assert.Single(report.Results);
            Assert.Contains("orphan_key", report.Results[0].OrphanKeys);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_WithTempDirectory_DetectsEmptyValues()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create base file (en.json)
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello",
                    "world": "World"
                }
                """);

            // Create target file (tr.json) with empty value
            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba",
                    "world": ""
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.True(report.HasIssues);
            Assert.Single(report.Results);
            Assert.Contains("world", report.Results[0].EmptyValues);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_WithPlaceholderMismatch_DetectsMismatch()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create base file (en.json)
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "greeting": "Hello {name}!"
                }
                """);

            // Create target file (tr.json) without placeholder
            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "greeting": "Merhaba!"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                CheckPlaceholders = true,
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.True(report.HasIssues);
            Assert.Single(report.Results);
            Assert.Single(report.Results[0].PlaceholderMismatches);
            Assert.Equal("greeting", report.Results[0].PlaceholderMismatches[0].Key);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_WithMultipleTargets_ScansAllCultures()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create base file (en.json)
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello",
                    "world": "World"
                }
                """);

            // Create target files with different issues
            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba"
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "de.json"), """
                {
                    "hello": "",
                    "world": "Welt"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr", "de"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.True(report.HasIssues);
            Assert.Equal(2, report.Results.Count);
            Assert.Equal(1, report.TotalMissingKeys); // tr missing "world"
            Assert.Equal(1, report.TotalEmptyValues); // de has empty "hello"
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_WithIgnorePatterns_IgnoresMatchingFiles()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create files
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello"
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba"
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "ignored.en.json"), """
                {
                    "extra": "Extra key"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                IgnorePatterns = ["ignored"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            // Should not find any issues because only en.json and tr.json are considered
            // and they match
            Assert.False(report.HasIssues);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_NoIssues_ReturnsCleanReport()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create matching files
            File.WriteAllText(Path.Combine(tempDir, "en.json"), """
                {
                    "hello": "Hello",
                    "world": "World"
                }
                """);

            File.WriteAllText(Path.Combine(tempDir, "tr.json"), """
                {
                    "hello": "Merhaba",
                    "world": "Dünya"
                }
                """);

            ScanOptions options = new()
            {
                BaseCulture = "en",
                TargetCultures = ["tr"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            Assert.False(report.HasIssues);
            Assert.Equal(0, report.TotalMissingKeys);
            Assert.Equal(0, report.TotalOrphanKeys);
            Assert.Equal(0, report.TotalEmptyValues);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_NeutralResxAsBase_RecognisesFileAsBaseCulture()
    {
        // Issue #24: Projects with a non-English neutral language store base
        // strings in Resources.resx (no culture suffix) and translations in
        // Resources.en.resx, Resources.es.resx, etc. The base file should be
        // discovered as the configured base culture.
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_neutral_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Neutral base file (German) — no culture suffix
            File.WriteAllText(Path.Combine(tempDir, "Resources.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hallo</value></data>
                  <data name="World"><value>Welt</value></data>
                  <data name="Goodbye"><value>Auf Wiedersehen</value></data>
                </root>
                """);

            // English translation, missing one key
            File.WriteAllText(Path.Combine(tempDir, "Resources.en.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hello</value></data>
                  <data name="World"><value>World</value></data>
                </root>
                """);

            ScanOptions options = new()
            {
                BaseCulture = "de",
                TargetCultures = ["en"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            CultureComparisonResult enResult = Assert.Single(report.Results);
            Assert.Equal("en", enResult.Culture);
            Assert.Single(enResult.MissingKeys);
            Assert.Equal("Goodbye", enResult.MissingKeys[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Scan_NeutralFileWithoutBaseCultureMatch_StillIgnored()
    {
        // When BaseCulture is set but a neutral file's content does not match,
        // the file is grouped under BaseCulture. Any non-base culture files
        // remain comparable. This guards against losing files entirely.
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_scan_neutral_no_base_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Suffix-less file present, but BaseCulture targets a different value
            File.WriteAllText(Path.Combine(tempDir, "Resources.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hallo</value></data>
                </root>
                """);

            File.WriteAllText(Path.Combine(tempDir, "Resources.en.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hello</value></data>
                </root>
                """);

            // BaseCulture is "fr" — neutral file gets folded into "fr" group,
            // but no actual French translations exist for comparison.
            ScanOptions options = new()
            {
                BaseCulture = "fr",
                TargetCultures = ["en"],
                Recursive = false
            };

            ScanReport report = _service.Scan(tempDir, options);

            // Neutral file is treated as fr base, en is compared against it.
            CultureComparisonResult enResult = Assert.Single(report.Results);
            Assert.Equal("en", enResult.Culture);
            Assert.Empty(enResult.MissingKeys);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}