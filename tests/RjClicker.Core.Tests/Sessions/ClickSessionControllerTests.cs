using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;

namespace RjClicker.Core.Tests.Sessions;

public sealed class ClickSessionControllerTests
{
    [Fact]
    public async Task StartAsync_ShouldDispatchClicks_AndStopAtCounterLimit()
    {
        var dispatcher = new FakeDispatcher();
        var scheduler = new FakeScheduler(maxTicks: 10);
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 1,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: true,
            maxClicks: 2,
            targets: new[] { PointTarget.Absolute(1, 1) });

        await controller.StartAsync(config, CancellationToken.None);

        dispatcher.DispatchCalls.Should().Be(2);
        controller.IsRunning.Should().BeFalse();
    }
}
