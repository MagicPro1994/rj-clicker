using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;

namespace RjClicker.Core.Tests.Sessions;

internal sealed class FakeDispatcher : IClickDispatcher
{
    public int DispatchCalls { get; private set; }

    public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        DispatchCalls++;
        return Task.CompletedTask;
    }
}

internal sealed class AsyncFakeDispatcher : IClickDispatcher
{
    public int DispatchCalls { get; private set; }

    public async Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        DispatchCalls++;
        // Simulate async work
        await Task.Delay(1, cancellationToken);
    }
}

internal sealed class FakeScheduler : IClickScheduler
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

internal sealed class RealSchedulerWithDelay : IClickScheduler
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
            // Expected
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
            // Expected
        }
    }
}

internal sealed class SlowFakeScheduler : IClickScheduler
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
            // Expected
        }
    }
}
