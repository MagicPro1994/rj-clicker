using FluentAssertions;
using RjClicker.App.Services;
using System.IO;
using System.Text.RegularExpressions;

namespace RjClicker.Core.Tests.Services;

public sealed class FileAppLoggerTests
{
    [Fact]
    public async Task LogErrorAsync_ShouldAppendMessageAndExceptionToConfiguredFile()
    {
        var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectoryPath);
        var logPath = Path.Combine(temporaryDirectoryPath, "rjclicker.log");

        try
        {
            var logger = new FileAppLogger(logPath);
            var exception = new InvalidOperationException("boom");

            await logger.LogErrorAsync("JsonSettingsStore", "Failed to save settings", exception);

            var logContent = await File.ReadAllTextAsync(logPath);
            logContent.Should().Contain("ERROR [JsonSettingsStore] Failed to save settings");
            logContent.Should().Contain("InvalidOperationException");
            logContent.Should().Contain("boom");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task DefaultConstructor_ShouldWriteToBaseDirectoryLogFile()
    {
        var expectedPath = Path.Combine(AppContext.BaseDirectory, "rjclicker.log");

        try
        {
            if (File.Exists(expectedPath))
            {
                File.Delete(expectedPath);
            }

            var logger = new FileAppLogger();

            await logger.LogErrorAsync("App", "probe");

            File.Exists(expectedPath).Should().BeTrue();
            var logContent = await File.ReadAllTextAsync(expectedPath);
            logContent.Should().Contain("ERROR [App] probe");
        }
        finally
        {
            if (File.Exists(expectedPath))
            {
                File.Delete(expectedPath);
            }
        }
    }

    [Fact]
    public async Task LogErrorAsync_ShouldSuppressIoFailures()
    {
        var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectoryPath);
        var logPath = Path.Combine(temporaryDirectoryPath, "rjclicker.log");
        FileStream? lockedLogStream = null;

        try
        {
            await File.WriteAllTextAsync(logPath, string.Empty);
            lockedLogStream = new FileStream(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var logger = new FileAppLogger(logPath);

            var logAction = async () => await logger.LogErrorAsync("App", "probe");

            await logAction.Should().NotThrowAsync();
        }
        finally
        {
            lockedLogStream?.Dispose();

            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task LogErrorAsync_ShouldPrefixEntriesWithUtcTimestamp()
    {
        var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectoryPath);
        var logPath = Path.Combine(temporaryDirectoryPath, "rjclicker.log");

        try
        {
            var logger = new FileAppLogger(logPath);

            await logger.LogErrorAsync("App", "timestamp-check");

            var logLines = await File.ReadAllLinesAsync(logPath);
            logLines.Should().NotBeEmpty();
            logLines[0].Should().MatchRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} ERROR \[App\] timestamp-check$");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task LogErrorAsync_ShouldAppendAcrossMultipleCalls()
    {
        var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectoryPath);
        var logPath = Path.Combine(temporaryDirectoryPath, "rjclicker.log");

        try
        {
            var logger = new FileAppLogger(logPath);

            await logger.LogErrorAsync("App", "first-entry");
            await logger.LogErrorAsync("App", "second-entry");

            var logContent = await File.ReadAllTextAsync(logPath);
            var firstIndex = logContent.IndexOf("ERROR [App] first-entry", StringComparison.Ordinal);
            var secondIndex = logContent.IndexOf("ERROR [App] second-entry", StringComparison.Ordinal);

            firstIndex.Should().BeGreaterThanOrEqualTo(0);
            secondIndex.Should().BeGreaterThan(firstIndex);
            Regex.Matches(logContent, @"ERROR \[App\]").Count.Should().Be(2);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectoryPath))
            {
                Directory.Delete(temporaryDirectoryPath, recursive: true);
            }
        }
    }
}
