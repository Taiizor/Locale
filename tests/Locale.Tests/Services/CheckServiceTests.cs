using Locale.Formats;
using Locale.Models;
using Locale.Services;

namespace Locale.Tests.Services;

public class CheckServiceTests
{
    private readonly CheckService _service = new();

    [Fact]
    public void Check_NoViolations_ReturnsEmptyReport()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        CheckReport report = _service.Check(file);

        Assert.False(report.HasViolations);
    }

    [Fact]
    public void Check_EmptyValue_ReportsViolation()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "" },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        CheckOptions options = new() { Rules = [CheckRules.NoEmptyValues] };
        CheckReport report = _service.Check(file, options);

        Assert.True(report.HasViolations);
        Assert.Single(report.Violations);
        Assert.Equal(CheckRules.NoEmptyValues, report.Violations[0].RuleName);
        Assert.Equal("hello", report.Violations[0].Key);
    }

    [Fact]
    public void Check_TrailingWhitespace_ReportsViolation()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello " },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        CheckOptions options = new() { Rules = [CheckRules.NoTrailingWhitespace] };
        CheckReport report = _service.Check(file, options);

        Assert.True(report.HasViolations);
        Assert.Single(report.Violations);
        Assert.Equal(CheckRules.NoTrailingWhitespace, report.Violations[0].RuleName);
        Assert.Equal("hello", report.Violations[0].Key);
    }

    [Fact]
    public void Check_AllRules_ChecksMultipleRules()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "empty", Value = "" },
                new LocalizationEntry { Key = "trailing", Value = "Value " }
            ]
        };

        CheckOptions options = new() { Rules = [CheckRules.NoEmptyValues, CheckRules.NoTrailingWhitespace] };
        CheckReport report = _service.Check(file, options);

        Assert.True(report.HasViolations);
        Assert.Equal(2, report.ViolationCount);
    }

    [Fact]
    public void Check_NullValue_ReportsEmptyViolation()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "null", Value = null }
            ]
        };

        CheckOptions options = new() { Rules = [CheckRules.NoEmptyValues] };
        CheckReport report = _service.Check(file, options);

        Assert.True(report.HasViolations);
        Assert.Single(report.Violations);
    }

    [Fact]
    public void Check_DuplicateKeys_ReportsViolation()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "duplicate", Value = "First" },
                new LocalizationEntry { Key = "duplicate", Value = "Second" }
            ]
        };

        CheckOptions options = new() { Rules = [CheckRules.NoDuplicateKeys] };
        CheckReport report = _service.Check(file, options);

        Assert.True(report.HasViolations);
        Assert.Single(report.Violations);
        Assert.Equal(CheckRules.NoDuplicateKeys, report.Violations[0].RuleName);
        Assert.Equal("duplicate", report.Violations[0].Key);
        Assert.Equal(ViolationSeverity.Error, report.Violations[0].Severity);
    }

    [Fact]
    public void Check_WithDefaultOptions_ChecksAllRules()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "ok", Value = "OK" }
            ]
        };

        CheckReport report = _service.Check(file);

        Assert.False(report.HasViolations);
    }

    [Fact]
    public void Check_WithNullOptions_UsesDefaults()
    {
        LocalizationFile file = new()
        {
            FilePath = "test.json",
            Culture = "en",
            Entries =
            [
                new LocalizationEntry { Key = "test", Value = "Test" }
            ]
        };

        CheckReport report = _service.Check(file, null);

        Assert.False(report.HasViolations);
    }

    [Fact]
    public void CheckOptions_Properties_ShouldWorkAsExpected()
    {
        CheckOptions options = new()
        {
            Rules = [CheckRules.NoEmptyValues, CheckRules.NoDuplicateKeys],
            BaseCulture = "en",
            Recursive = false,
            PlaceholderPattern = @"\{(\w+)\}"
        };

        Assert.Equal(2, options.Rules.Count);
        Assert.Equal("en", options.BaseCulture);
        Assert.False(options.Recursive);
        Assert.Equal(@"\{(\w+)\}", options.PlaceholderPattern);
    }

    [Fact]
    public void CheckRules_AllConstantsShouldExist()
    {
        Assert.Equal("no-empty-values", CheckRules.NoEmptyValues);
        Assert.Equal("no-duplicate-keys", CheckRules.NoDuplicateKeys);
        Assert.Equal("no-orphan-keys", CheckRules.NoOrphanKeys);
        Assert.Equal("consistent-placeholders", CheckRules.ConsistentPlaceholders);
        Assert.Equal("no-trailing-whitespace", CheckRules.NoTrailingWhitespace);
    }

    [Fact]
    public void CheckRules_AllArray_ShouldContainAllRules()
    {
        Assert.Equal(5, CheckRules.All.Length);
        Assert.Contains(CheckRules.NoEmptyValues, CheckRules.All);
        Assert.Contains(CheckRules.NoDuplicateKeys, CheckRules.All);
        Assert.Contains(CheckRules.NoOrphanKeys, CheckRules.All);
        Assert.Contains(CheckRules.ConsistentPlaceholders, CheckRules.All);
        Assert.Contains(CheckRules.NoTrailingWhitespace, CheckRules.All);
    }

    [Fact]
    public void Constructor_WithRegistry_ShouldWork()
    {
        CheckService service = new(FormatRegistry.Default);
        Assert.NotNull(service);
    }

    [Fact]
    public void Check_NeutralFileAsBase_DetectsOrphanKeysInTranslation()
    {
        // Issue #24: a neutral Resources.resx (Culture=null) should be treated
        // as the base culture file when --base is supplied to `locale check`.
        string tempDir = Path.Combine(Path.GetTempPath(), $"locale_check_neutral_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            // German neutral file — base
            File.WriteAllText(Path.Combine(tempDir, "Resources.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hallo</value></data>
                </root>
                """);

            // English file with an orphan key
            File.WriteAllText(Path.Combine(tempDir, "Resources.en.resx"), """
                <?xml version="1.0" encoding="utf-8"?>
                <root>
                  <data name="Hello"><value>Hello</value></data>
                  <data name="Orphan"><value>Not in base</value></data>
                </root>
                """);

            CheckOptions options = new()
            {
                Rules = [CheckRules.NoOrphanKeys],
                BaseCulture = "de"
            };

            CheckReport report = _service.Check(tempDir, options);

            CheckViolation violation = Assert.Single(report.Violations);
            Assert.Equal(CheckRules.NoOrphanKeys, violation.RuleName);
            Assert.Equal("Orphan", violation.Key);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}