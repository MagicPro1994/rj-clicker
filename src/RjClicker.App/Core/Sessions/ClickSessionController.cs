using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickSessionController
{
    private readonly IClickDispatcher _dispatcher;
    private readonly IClickScheduler _scheduler;

    public ClickSessionController(IClickDispatcher dispatcher, IClickScheduler scheduler)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (IsRunning)
        {
            throw new InvalidOperationException("Session already running");
        }

        IsRunning = true;
        var clickCount = 0;

        await _scheduler.RunAsync(
            config.TotalIntervalMilliseconds,
            onTick: () =>
            {
                if (HasReachedCounterLimit(config, clickCount))
                {
                    IsRunning = false;
                    return;
                }

                _dispatcher.DispatchAsync(config, cancellationToken).GetAwaiter().GetResult();
                clickCount++;

                if (HasReachedCounterLimit(config, clickCount))
                {
                    IsRunning = false;
                }
            },
            cancellationToken);

        IsRunning = false;
    }

    private static bool HasReachedCounterLimit(RuntimeConfig config, int clickCount)
    {
        return config.UseCounter && config.MaxClicks.HasValue && clickCount >= config.MaxClicks.Value;
    }
}
