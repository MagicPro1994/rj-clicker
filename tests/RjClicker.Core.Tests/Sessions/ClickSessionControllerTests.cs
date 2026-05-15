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

        await Task.Delay(10);
        controller.IsRunning.Should().BeTrue();

        controller.Stop();
        await startTask;

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

        await Task.Delay(10);

        var exception = await Record.ExceptionAsync(() => controller.StartAsync(config, CancellationToken.None));
        exception.Should().BeOfType<InvalidOperationException>();

        cts.Cancel();
        await startTask;
    }

    [Fact]
    public void IsRunning_ShouldBeFalse_WhenNotStarted()
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

        await Task.Delay(10);
        controller.IsRunning.Should().BeTrue();

        controller.Stop();
        await startTask;

        controller.IsRunning.Should().BeFalse();
    }

    private sealed class FakeDispatcher : IClickDispatcher
    {
        public int DispatchCalls { get; private set; }

        public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
        {
            DispatchCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class AsyncFakeDispatcher : IClickDispatcher
    {
        public int DispatchCalls { get; private set; }

        public async Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
        {
            DispatchCalls++;
            await Task.Delay(1, cancellationToken);
        }
    }

    private sealed class FakeScheduler : IClickScheduler
    {
        private readonly int _maxTicks;

        public FakeScheduler(int maxTicks)
        {
            _maxTicks = maxTicks;
        }

        public Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            for (var tick = 0; tick < _maxTicks; tick++)
            {
                onTick();
            }

            return Task.CompletedTask;
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            for (var tick = 0; tick < _maxTicks && !cancellationToken.IsCancellationRequested; tick++)
            {
                await onTick();
            }
        }
    }

    private sealed class RealSchedulerWithDelay : IClickScheduler
    {
        public async Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    onTick();
                    await Task.Delay(intervalMilliseconds, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async Task RunAsyncWithAsyncHandler(int intervalMilliseconds, Func<Task> onTick, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await onTick();
                    await Task.Delay(intervalMilliseconds, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private sealed class SlowFakeScheduler : IClickScheduler
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
