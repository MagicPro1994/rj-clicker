using FluentAssertions;
using RjClicker.App.Core.Sessions;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickSchedulerTests
{
    [Fact]
    public async Task RunAsync_ShouldInvokeTickHandler_UntilCancelled()
    {
        var ticks = 0;
        using var cts = new CancellationTokenSource();

        var scheduler = new ClickScheduler();
        var runTask = scheduler.RunAsync(
            intervalMilliseconds: 5,
            onTick: () =>
            {
                ticks++;
                if (ticks >= 3)
                {
                    cts.Cancel();
                }
            },
            cts.Token);

        await runTask;

        ticks.Should().BeGreaterThanOrEqualTo(3);
    }
}
