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
}