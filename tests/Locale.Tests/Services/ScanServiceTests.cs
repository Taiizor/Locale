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
}