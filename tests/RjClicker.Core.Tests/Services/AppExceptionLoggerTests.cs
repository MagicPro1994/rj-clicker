using FluentAssertions;
using RjClicker.App.Services;

namespace RjClicker.Core.Tests.Services;

public sealed class AppExceptionLoggerTests
{
    [Fact]
    public async Task LogDispatcherUnhandledExceptionAsync_ShouldWriteThroughLogger()
    {
        var logger = new SpyAppLogger();
        var service = new AppExceptionLogger(logger);
        var exception = new InvalidOperationException("dispatcher failed");

        await service.LogDispatcherUnhandledExceptionAsync(exception);

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Source.Should().Be("App");
        logger.Entries[0].Message.Should().Be("Unhandled dispatcher exception");
        logger.Entries[0].Exception.Should().Be(exception);
    }

    private sealed class SpyAppLogger : IAppLogger
    {
        public List<(string Source, string Message, Exception? Exception)> Entries { get; } = [];

        public Task LogErrorAsync(string source, string message, Exception? exception = null)
        {
            Entries.Add((source, message, exception));
            return Task.CompletedTask;
        }
    }
}
