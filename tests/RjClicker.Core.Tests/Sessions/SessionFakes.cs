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
}
