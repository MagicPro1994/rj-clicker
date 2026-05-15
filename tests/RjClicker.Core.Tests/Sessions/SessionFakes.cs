using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Hotkeys;
using System.Windows.Input;

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

internal sealed class FakeHotkeyService : IGlobalHotkeyService
{
    public List<int> RegisteredHotkeys { get; } = [];
    public List<int> UnregisteredHotkeys { get; } = [];
    public ModifierKeys LastRegisteredModifiers { get; private set; }
    public Key LastRegisteredKey { get; private set; }
    private readonly Dictionary<int, Func<Task>> _callbacks = [];

    public Task RegisterAsync(nint windowHandle, int hotkeyId, ModifierKeys modifiers, Key key, Func<Task> onPressed)
    {
        ArgumentNullException.ThrowIfNull(onPressed);
        RegisteredHotkeys.Add(hotkeyId);
        LastRegisteredModifiers = modifiers;
        LastRegisteredKey = key;
        _callbacks[hotkeyId] = onPressed;
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(int hotkeyId)
    {
        UnregisteredHotkeys.Add(hotkeyId);
        _callbacks.Remove(hotkeyId);
        return Task.CompletedTask;
    }

    public void HandleHotkeyPressed(int hotkeyId)
    {
        if (_callbacks.TryGetValue(hotkeyId, out var callback))
        {
            _ = callback();
        }
    }

    public Task TriggerHotkeyAsync(int hotkeyId)
    {
        if (!_callbacks.TryGetValue(hotkeyId, out var callback))
        {
            return Task.CompletedTask;
        }

        return callback();
    }
}
