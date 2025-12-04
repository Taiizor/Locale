using Locale.Models;
using Locale.Services;

namespace Locale.Tests.Services;

public class DiffServiceTests
{
    private readonly DiffService _service = new();

    [Fact]
    public void Diff_IdenticalFiles_NoDifferences()
    {
        LocalizationFile first = new()
        {
            FilePath = "first.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        LocalizationFile second = new()
        {
            FilePath = "second.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        DiffReport report = _service.Diff(first, second);

        Assert.False(report.HasDifferences);
        Assert.Empty(report.OnlyInFirst);
        Assert.Empty(report.OnlyInSecond);
        Assert.Empty(report.EmptyInSecond);
    }

    [Fact]
    public void Diff_MissingKeysInSecond_ReportsOnlyInFirst()
    {
        LocalizationFile first = new()
        {
            FilePath = "first.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" },
                new LocalizationEntry { Key = "world", Value = "World" }
            ]
        };

        LocalizationFile second = new()
        {
            FilePath = "second.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" }
            ]
        };

        DiffReport report = _service.Diff(first, second);

        Assert.True(report.HasDifferences);
        Assert.Single(report.OnlyInFirst);
        Assert.Contains("world", report.OnlyInFirst);
    }

    [Fact]
    public void Diff_ExtraKeysInSecond_ReportsOnlyInSecond()
    {
        LocalizationFile first = new()
        {
            FilePath = "first.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" }
            ]
        };

        LocalizationFile second = new()
        {
            FilePath = "second.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" },
                new LocalizationEntry { Key = "extra", Value = "Extra" }
            ]
        };

        DiffReport report = _service.Diff(first, second);

        Assert.True(report.HasDifferences);
        Assert.Single(report.OnlyInSecond);
        Assert.Contains("extra", report.OnlyInSecond);
    }

    [Fact]
    public void Diff_EmptyValuesInSecond_ReportsEmptyInSecond()
    {
        LocalizationFile first = new()
        {
            FilePath = "first.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "Hello" }
            ]
        };

        LocalizationFile second = new()
        {
            FilePath = "second.json",
            Entries =
            [
                new LocalizationEntry { Key = "hello", Value = "" }
            ]
        };

        DiffReport report = _service.Diff(first, second);

        Assert.True(report.HasDifferences);
        Assert.Single(report.EmptyInSecond);
        Assert.Contains("hello", report.EmptyInSecond);
    }

    [Fact]
    public void Diff_PlaceholderMismatch_ReportsMismatch()
    {
        LocalizationFile first = new()
        {
            FilePath = "first.json",
            Entries =
            [
                new LocalizationEntry { Key = "greeting", Value = "Hello {name}!" }
            ]
        };

        LocalizationFile second = new()
        {
            FilePath = "second.json",
            Entries =
            [
                new LocalizationEntry { Key = "greeting", Value = "Hello!" }
            ]
        };

        DiffReport report = _service.Diff(first, second);

        Assert.True(report.HasDifferences);
        Assert.Single(report.PlaceholderMismatches);
        Assert.Equal("greeting", report.PlaceholderMismatches[0].Key);
    }
}