using FluentAssertions;
using RjClicker.App.Services;
using System.IO;

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

    [Fact]
    public async Task LogErrorAsync_ShouldSuppressIoFailures()
    {
        var logger = new FileAppLogger(@"Z:\path-that-should-not-exist\rjclicker.log");

        var logAction = async () => await logger.LogErrorAsync("App", "probe");

        await logAction.Should().NotThrowAsync();
    }
}
