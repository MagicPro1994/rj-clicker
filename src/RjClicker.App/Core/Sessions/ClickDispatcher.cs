using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Sessions;

public interface IClickDispatcher
{
    Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken);
}

public sealed class ClickDispatcher : IClickDispatcher
{
    public Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);
        return Task.CompletedTask;
    }
}
