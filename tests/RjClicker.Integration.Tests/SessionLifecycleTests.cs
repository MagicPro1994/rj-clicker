using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;

namespace RjClicker.Integration.Tests;

public sealed class SessionLifecycleTests
{
    [Fact]
    public async Task StartStop_ShouldDispatchAndCompleteSession()
    {
        var dispatcher = new MockDispatcher();
        var scheduler = new MockScheduler();
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 10,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: true,
            maxClicks: 1,
            targets: new[] { PointTarget.Absolute(100, 100) });

        await controller.StartAsync(config, CancellationToken.None);

        dispatcher.DispatchCalls.Should().Be(1);
        controller.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Stop_ShouldCancelRunningSession()
    {
        var dispatcher = new MockDispatcher();
        var scheduler = new SlowScheduler();
        var controller = new ClickSessionController(dispatcher, scheduler);

        var config = new RuntimeConfig(
            MouseButton.Left,
            PressType.Single,
            totalIntervalMilliseconds: 10,
            ClickMode.Simultaneous,
            DeliveryMode.Foreground,
            useCounter: false,
            maxClicks: null,
            targets: new[] { PointTarget.Absolute(100, 100) });

        var startTask = controller.StartAsync(config, CancellationToken.None);

        await Task.Delay(25);
        controller.IsRunning.Should().BeTrue();

        controller.Stop();
        await startTask;

        controller.IsRunning.Should().BeFalse();
    }

    private sealed class MockDispatcher : IClickDispatcher
    {
        public int DispatchCalls { get; private set; }

        public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
        {
            DispatchCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class MockScheduler : IClickScheduler
    {
        public Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            onTick();
            return Task.CompletedTask;
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            await onTick();
        }
    }

    private sealed class SlowScheduler : IClickScheduler
    {
        public async Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                onTick();
                await Task.Delay(100, cancellationToken);
            }
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await onTick();
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
