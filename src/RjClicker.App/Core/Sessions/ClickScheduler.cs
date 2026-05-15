namespace RjClicker.App.Core.Sessions;

public interface IClickScheduler
{
    Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken);
}

public sealed class ClickScheduler : IClickScheduler
{
    public async Task RunAsync(int intervalMilliseconds, Action onTick, CancellationToken cancellationToken)
    {
        if (intervalMilliseconds < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds));
        }

        ArgumentNullException.ThrowIfNull(onTick);

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
            // Cancellation requested, exit gracefully
        }
    }
}
