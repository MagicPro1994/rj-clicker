using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public sealed class ClickSessionController
{
    private readonly IClickDispatcher _dispatcher;
    private readonly IClickScheduler _scheduler;
    private CancellationTokenSource? _sessionCancellation;

    public ClickSessionController(IClickDispatcher dispatcher, IClickScheduler scheduler)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
    }

    public bool IsRunning => _sessionCancellation is not null && !_sessionCancellation.Token.IsCancellationRequested;

    public async Task StartAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (IsRunning)
        {
            throw new InvalidOperationException("Session already running");
        }

        _sessionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var clickCount = 0;

            await _scheduler.RunAsyncWithAsyncHandler(
                config.TotalIntervalMilliseconds,
                onTick: async () =>
                {
                    if (HasReachedCounterLimit(config, clickCount))
                    {
                        _sessionCancellation.Cancel();
                        return;
                    }

                    await _dispatcher.DispatchAsync(config, _sessionCancellation.Token);
                    clickCount++;

                    if (HasReachedCounterLimit(config, clickCount))
                    {
                        _sessionCancellation.Cancel();
                    }
                },
                _sessionCancellation.Token);
        }
        finally
        {
            _sessionCancellation?.Dispose();
            _sessionCancellation = null;
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            _sessionCancellation?.Cancel();
        }
    }

    private static bool HasReachedCounterLimit(RuntimeConfig config, int clickCount)
    {
        return config.UseCounter && config.MaxClicks.HasValue && clickCount >= config.MaxClicks.Value;
    }
}
