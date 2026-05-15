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

    [Fact]
    public async Task StartAsync_ShouldHandleAsyncDispatch_WithoutBlocking()
    {
        var dispatcher = new AsyncFakeDispatcher();
        var scheduler = new FakeScheduler(maxTicks: 3);
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 1,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: true,
            maxClicks: 5,
            targets: new[] { PointTarget.Absolute(1, 1) });

        await controller.StartAsync(config, CancellationToken.None);

        dispatcher.DispatchCalls.Should().Be(3);
        controller.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Stop_ShouldCancelRunningSession()
    {
        var dispatcher = new AsyncFakeDispatcher();
        var scheduler = new RealSchedulerWithDelay();
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 50,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets: new[] { PointTarget.Absolute(1, 1) });

        var startTask = controller.StartAsync(config, CancellationToken.None);
        
        // Give it a moment to start
        await Task.Delay(10);
        
        // Verify session is running
        controller.IsRunning.Should().BeTrue();

        // Stop the session
        controller.Stop();

        // Wait for the session to complete
        await startTask;

        // Verify session is stopped
        controller.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_ShouldThrow_WhenAlreadyRunning()
    {
        var dispatcher = new FakeDispatcher();
        var scheduler = new SlowFakeScheduler();
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 1,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets: new[] { PointTarget.Absolute(1, 1) });

        var cts = new CancellationTokenSource();
        var startTask = controller.StartAsync(config, cts.Token);

        // Give it a moment to start
        await Task.Delay(10);

        // Try to start again while running
        var exception = await Record.ExceptionAsync(() => controller.StartAsync(config, CancellationToken.None));
        exception.Should().BeOfType<InvalidOperationException>();

        // Cleanup
        cts.Cancel();
        await startTask;
    }

    [Fact]
    public async Task IsRunning_ShouldBeFalse_WhenNotStarted()
    {
        var dispatcher = new FakeDispatcher();
        var scheduler = new FakeScheduler(maxTicks: 1);
        var controller = new ClickSessionController(dispatcher, scheduler);

        controller.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task IsRunning_ShouldReflectCancellation_WhenStopCalled()
    {
        var dispatcher = new AsyncFakeDispatcher();
        var scheduler = new RealSchedulerWithDelay();
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 100,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets: new[] { PointTarget.Absolute(1, 1) });

        var startTask = controller.StartAsync(config, CancellationToken.None);

        // Give it a moment to start
        await Task.Delay(10);
        controller.IsRunning.Should().BeTrue();

        controller.Stop();
        await startTask;

        controller.IsRunning.Should().BeFalse();
    }
}
